using System;
using System.ServiceModel;
using log4net;
using Rhino.Licensing.Contracts;

namespace Rhino.Licensing.Wcf
{
    /// <summary>
    /// Floating license provider that retrieves floating licenses from a WCF Service
    /// </summary>
    public class FloatingLicenseProvider: IFloatingLicenseProvider
    {
        private readonly string licenseServerUrl;
        private readonly Guid clientId;
        
        /// <summary>
        /// License validator logger
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(FloatingLicenseProvider));

        /// <summary>
        /// Creates a floating license provider that connects to the specified WCF service.
        /// </summary>
        /// <param name="licenseServerUrl">URL to WCF service provider for ILicensingService</param>
        /// <param name="clientId"></param>
        public FloatingLicenseProvider(string licenseServerUrl, Guid clientId)
        {
            if (string.IsNullOrEmpty(licenseServerUrl))
                throw new ArgumentException(nameof(licenseServerUrl));
            if (clientId == null)
                throw new ArgumentNullException(nameof(clientId));
            
            this.licenseServerUrl = licenseServerUrl;
            this.clientId         = clientId;
        }
        
        /// <summary>
        /// Gets a floating license from the configured WCF service
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="FloatingLicenseNotAvailableException"></exception>
        public string GetFloatingLicense()
        {
            if (licenseServerUrl == null)
            {
                Log.Warn("Could not find license server url");
                throw new InvalidOperationException("Floating license encountered, but licenseServerUrl was not set");
            }
            
            var licensingService = ChannelFactory<ILicensingService>.CreateChannel(new WSHttpBinding(), new EndpointAddress(licenseServerUrl));

            var success = false;
            try
            {
                var floatingLicense = licensingService.LeaseLicense(
                    Environment.MachineName,
                    Environment.UserName,
                    clientId);
                ((ICommunicationObject)licensingService).Close();
                success = true;

                if (floatingLicense != null) return floatingLicense;
                
                Log.WarnFormat("Null response from license server: {0}", licenseServerUrl);
                throw new FloatingLicenseNotAvailableException();
            }
            finally
            {
                if (success == false)
                    ((ICommunicationObject)licensingService).Abort();
            }
        }
    }
}
