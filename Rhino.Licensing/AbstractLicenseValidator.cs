﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Threading;
using System.Xml;
using log4net;
using Rhino.Licensing.Contracts;
using Rhino.Licensing.Discovery;

namespace Rhino.Licensing
{
    /// <summary>
    /// Base license validator.
    /// </summary>
    public abstract class AbstractLicenseValidator: ILicenseValidator
    {
        /// <summary>
        /// License validator logger
        /// </summary>
        protected static readonly ILog Log = LogManager.GetLogger(typeof(LicenseValidator));

        /// <summary>
        /// Standard Time servers
        /// </summary>
        protected static readonly string[] TimeServers =
        {
            "time.nist.gov",
            "time-nw.nist.gov",
            "time-a.nist.gov",
            "time-b.nist.gov",
            "time-a.timefreq.bldrdoc.gov",
            "time-b.timefreq.bldrdoc.gov",
            "time-c.timefreq.bldrdoc.gov",
            "utcnist.colorado.edu",
            "nist1.datum.com",
            "nist1.dc.certifiedtime.com",
            "nist1.nyc.certifiedtime.com",
            "nist1.sjc.certifiedtime.com"
        };

        private readonly string publicKey;
        private readonly Timer nextLeaseTimer;
        private bool disableFutureChecks;
        private bool currentlyValidatingSubscriptionLicense;
        private readonly DiscoveryHost discoveryHost;
        private DiscoveryClient discoveryClient;
        private Guid senderId;

        /// <summary>
        /// Fired when license data is invalidated
        /// </summary>
        public event Action<InvalidationType> LicenseInvalidated;

        /// <summary>
        /// Fired when license is expired
        /// </summary>
        public event Action<DateTime> LicenseExpired;

        /// <summary>
        /// Event that's raised when duplicate licenses are found
        /// </summary>
        public event EventHandler<DiscoveryHost.ClientDiscoveredEventArgs> MultipleLicensesWereDiscovered;

        /// <summary>
        /// Disable the <see cref="ExpirationDate"/> validation with the time servers
        /// </summary>
        public bool DisableTimeServersCheck
        {
            get; set;
        }
        
        /// <summary>
        /// Gets the expiration date of the license
        /// </summary>
        public DateTime ExpirationDate
        {
            get; private set;
        }

        /// <summary>
        /// Lease timeout
        /// </summary>
        public TimeSpan LeaseTimeout { get; set; }

        /// <summary>
        /// How to behave when using the same license multiple times
        /// </summary>
        public MultipleLicenseUsage MultipleLicenseUsageBehavior { get; set; }

        /// <summary>
        /// Gets or sets the provider for subscription leases
        /// </summary>
        public ISubscriptionLeaseProvider SubscriptionLeaseProvider
        {
            get; set;
        }

        /// <summary>
        /// Gets the Type of the license
        /// </summary>
        public LicenseType LicenseType
        {
            get; private set;
        }

        /// <summary>
        /// Gets the Id of the license holder
        /// </summary>
        public Guid UserId
        {
            get; private set;
        }

        /// <summary>
        /// Gets the name of the license holder
        /// </summary>
        public string Name
        {
            get; private set;
        }

        /// <summary>
        /// Gets or Sets Floating license support
        /// </summary>
        public bool DisableFloatingLicenses
        {
            get; set;
        }

        /// <summary>
        /// Gets or Sets a provider for floating licenses
        /// </summary>
        public IFloatingLicenseProvider FloatingLicenseProvider
        {
            get; set;
        }

        /// <summary>
        /// Whether the client discovery server is enabled. This detects duplicate licenses used on the same network.
        /// </summary>
        public bool DiscoveryEnabled { get; private set; }

        /// <summary>
        /// Gets extra license information
        /// </summary>
        public IDictionary<string, string> LicenseAttributes
        {
            get; private set;
        }

        /// <summary>
        /// Gets or Sets the license content
        /// </summary>
        protected abstract string License
        {
            get; set;
        }

        /// <summary>
        /// Creates a license validator with specfied public key.
        /// </summary>
        /// <param name="publicKey">public key</param>
        /// <param name="enableDiscovery">Whether to enable the client discovery server to detect duplicate licenses used on the same network.</param>
        protected AbstractLicenseValidator(string publicKey, bool enableDiscovery = true)
        {
            LeaseTimeout = TimeSpan.FromMinutes(5);
            LicenseAttributes = new Dictionary<string, string>();
            nextLeaseTimer = new Timer(LeaseLicenseAgain);
            this.publicKey = publicKey;

            DiscoveryEnabled = enableDiscovery;

            if (DiscoveryEnabled)
            {
                senderId = Guid.NewGuid();
                discoveryHost = new DiscoveryHost();
                discoveryHost.ClientDiscovered += DiscoveryHostOnClientDiscovered;
                discoveryHost.Start();
            }
        }

        private void LeaseLicenseAgain(object state)
        {
            var client = discoveryClient;
            if (client != null)
                client.PublishMyPresence();

            if (HasExistingLicense())
                return;

            RaiseLicenseInvalidated();
        }

        private void RaiseLicenseInvalidated()
        {
            var licenseInvalidated = LicenseInvalidated;
            if (licenseInvalidated == null)
                throw new InvalidOperationException("License was invalidated, but there is no one subscribe to the LicenseInvalidated event");

            licenseInvalidated(LicenseType == LicenseType.Floating ? InvalidationType.CannotGetNewLicense :
                                                                     InvalidationType.TimeExpired);
        }

        private void RaiseMultipleLicenseDiscovered(DiscoveryHost.ClientDiscoveredEventArgs args)
        {
            var onMultipleLicensesWereDiscovered = MultipleLicensesWereDiscovered;
            if (onMultipleLicensesWereDiscovered != null)
            {
                onMultipleLicensesWereDiscovered(this, args);
            }
        }

        private void DiscoveryHostOnClientDiscovered(object sender, DiscoveryHost.ClientDiscoveredEventArgs clientDiscoveredEventArgs)
        {
            if (senderId == clientDiscoveredEventArgs.SenderId) // we got our own notification, ignore it
                return;

            if (UserId != clientDiscoveredEventArgs.UserId) // another license, we don't care
                return;

            // same user id, different senders
            switch (MultipleLicenseUsageBehavior)
            {
                case MultipleLicenseUsage.AllowForSameUser:
                    if (Environment.UserName == clientDiscoveredEventArgs.UserName)
                        return;
                    break;
            }

            RaiseLicenseInvalidated();
            RaiseMultipleLicenseDiscovered(clientDiscoveredEventArgs);
        }

        /// <summary>
        /// Validates loaded license
        /// </summary>
        public virtual void AssertValidLicense()
        {
            LicenseAttributes.Clear();
            if (HasExistingLicense())
            {
                if (DiscoveryEnabled)
                {
                    discoveryClient = new DiscoveryClient(senderId, UserId, Environment.MachineName, Environment.UserName);
                    discoveryClient.PublishMyPresence();
                }
                return;
            }

            Log.WarnFormat("Could not validate existing license\r\n{0}", License);
            throw new LicenseNotFoundException();
        }

        private bool HasExistingLicense()
        {
            try
            {
                if (TryLoadingLicenseValuesFromValidatedXml() == false)
                {
                    Log.WarnFormat("Failed validating license:\r\n{0}", License);
                    return false;
                }
                Log.InfoFormat("License expiration date is {0}", ExpirationDate);

                bool result;
                if (LicenseType == LicenseType.Subscription)
                {
                    result = ValidateSubscription();
                }
                else
                {
                    result = DateTime.UtcNow < ExpirationDate;
                }

                if (result &&
                    !DisableTimeServersCheck)
                {
                    ValidateUsingNetworkTime();
                }

                if (!result)
                {
                    if (LicenseExpired == null)
                        throw new LicenseExpiredException("Expiration Date : " + ExpirationDate);

                    DisableFutureChecks();
                    LicenseExpired(ExpirationDate);
                }

                return true;
            }
            catch (RhinoLicensingException)
            {
                throw;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool ValidateSubscription()
        {
            if ((ExpirationDate - DateTime.UtcNow).TotalDays > 4)
                return true;

            if (currentlyValidatingSubscriptionLicense)
                return DateTime.UtcNow < ExpirationDate;

            try
            {
                if (SubscriptionLeaseProvider == null)
                {
                    Log.Warn("Lease subscription provider has not been configured.");
                    return false;
                }

                var newLicense = SubscriptionLeaseProvider.GetLeaseSubscription(License);
                if (IsValidXml(newLicense))
                {
                    License = newLicense;
                }
            }
            catch (Exception e)
            {
                Log.Error("Could not re-lease subscription license", e);
            }

            return ValidateWithoutUsingSubscriptionLeasing();
        }

        private bool ValidateWithoutUsingSubscriptionLeasing()
        {
            currentlyValidatingSubscriptionLicense = true;
            try
            {
                return HasExistingLicense();
            }
            finally
            {
                currentlyValidatingSubscriptionLicense = false;
            }
        }
        
        /// <summary>
        /// Determines whether the specified license contains valid XML or not. 
        /// </summary>
        /// <param name="newLicense"></param>
        /// <returns></returns>
        protected bool IsValidXml(string newLicense)
        {
            if (string.IsNullOrEmpty(newLicense))
                return false;
            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(newLicense);
            }
            catch (Exception e)
            {
                Log.Error("New license is not valid XML\r\n" + newLicense, e);
                return false;
            }
            return true;
        }

        private void ValidateUsingNetworkTime()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return;

            var sntp = new SntpClient(GetTimeServers());
            sntp.BeginGetDate(time =>
            {
                if (time > ExpirationDate)
                    RaiseLicenseInvalidated();
            }
            , () =>
            {
                /* ignored */
            });
        }

        /// <summary>
        /// Extension point to return different time servers
        /// </summary>
        /// <returns></returns>
        protected virtual string[] GetTimeServers()
        {
            return TimeServers;
        }

        /// <summary>
        /// Removes existing license from the machine.
        /// </summary>
        public virtual void RemoveExistingLicense()
        {
        }

        /// <summary>
        /// Loads license data from validated license file.
        /// </summary>
        /// <returns></returns>
        public bool TryLoadingLicenseValuesFromValidatedXml()
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(License);

                if (TryGetValidDocument(publicKey, doc) == false)
                {
                    Log.WarnFormat("Could not validate xml signature of:\r\n{0}", License);
                    return false;
                }

                if (doc.FirstChild == null)
                {
                    Log.WarnFormat("Could not find first child of:\r\n{0}", License);
                    return false;
                }

                if (doc.SelectSingleNode("/floating-license") != null)
                {
                    var node = doc.SelectSingleNode("/floating-license/license-server-public-key/text()");
                    if (node == null)
                    {
                        Log.WarnFormat("Invalid license, floating license without license server public key:\r\n{0}", License);
                        throw new InvalidOperationException(
                            "Invalid license file format, floating license without license server public key");
                    }
                    return ValidateFloatingLicense(node.InnerText);
                }

                var result = ValidateXmlDocumentLicense(doc);
                if (result && disableFutureChecks == false)
                {
                    nextLeaseTimer.Change(LeaseTimeout, LeaseTimeout);
                }
                return result;
            }
            catch (RhinoLicensingException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error("Could not validate license", e);
                return false;
            }
        }

        private bool ValidateFloatingLicense(string publicKeyOfFloatingLicense)
        {
            if (DisableFloatingLicenses)
            {
                Log.Warn("Floating licenses have been disabled");
                return false;
            }

            if (FloatingLicenseProvider == null)
            {
                Log.Warn("Floating license provider has not been configured.");
                return false;
            }
            
            var leasedLicense = FloatingLicenseProvider.GetFloatingLicense();

            var doc = new XmlDocument();
            doc.LoadXml(leasedLicense);

            if (TryGetValidDocument(publicKeyOfFloatingLicense, doc) == false)
            {
                Log.WarnFormat("Could not get valid license from floating license server");
                throw new FloatingLicenseNotAvailableException();
            }

            var validLicense = ValidateXmlDocumentLicense(doc);
            if (validLicense)
            {
                //setup next lease
                var time = (ExpirationDate.AddMinutes(-5) - DateTime.UtcNow);
                Log.DebugFormat("Will lease license again at {0}", time);
                if (disableFutureChecks == false)
                    nextLeaseTimer.Change(time, time);
            }

            return validLicense;
        }

        internal bool ValidateXmlDocumentLicense(XmlDocument doc)
        {
            var id = doc.SelectSingleNode("/license/@id");
            if (id == null)
            {
                Log.WarnFormat("Could not find id attribute in license:\r\n{0}", License);
                return false;
            }

            UserId = new Guid(id.Value);

            var date = doc.SelectSingleNode("/license/@expiration");
            if (date == null)
            {
                Log.WarnFormat("Could not find expiration in license:\r\n{0}", License);
                return false;
            }

            ExpirationDate = DateTime.ParseExact(date.Value, "yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture);

            var licenseType = doc.SelectSingleNode("/license/@type");
            if (licenseType == null)
            {
                Log.WarnFormat("Could not find license type in {0}", licenseType);
                return false;
            }

            LicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), licenseType.Value);

            var name = doc.SelectSingleNode("/license/name/text()");
            if (name == null)
            {
                Log.WarnFormat("Could not find licensee's name in license:\r\n{0}", License);
                return false;
            }

            Name = name.Value;

            var license = doc.SelectSingleNode("/license");
            foreach (XmlAttribute attrib in license.Attributes)
            {
                if (attrib.Name == "type" || attrib.Name == "expiration" || attrib.Name == "id")
                    continue;

                LicenseAttributes[attrib.Name] = attrib.Value;
            }

            return true;
        }

        private bool TryGetValidDocument(string licensePublicKey, XmlDocument doc)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(licensePublicKey);

            var nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("sig", "http://www.w3.org/2000/09/xmldsig#");

            var signedXml = new SignedXml(doc);
            var sig = (XmlElement)doc.SelectSingleNode("//sig:Signature", nsMgr);
            if (sig == null)
            {
                Log.WarnFormat("Could not find this signature node on license:\r\n{0}", License);
                return false;
            }
            signedXml.LoadXml(sig);

            return signedXml.CheckSignature(rsa);
        }

        /// <summary>
        /// Disables further license checks for the session.
        /// </summary>
        public void DisableFutureChecks()
        {
            disableFutureChecks = true;
            nextLeaseTimer.Dispose();
        }

        /// <summary>
        /// Options for detecting multiple licenses
        /// </summary>
        public enum MultipleLicenseUsage
        {
            /// <summary>
            /// Deny if multiple licenses are used
            /// </summary>
            Deny,
            /// <summary>
            /// Only allow if it is running for the same user
            /// </summary>
            AllowForSameUser
        }
    }
}
