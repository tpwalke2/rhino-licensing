using System;
using System.ServiceModel;
using Rhino.Licensing.Contracts;

namespace Rhino.Licensing.Wcf
{
    /// <summary>
    /// Subscription lease provider that retrieves subscription leases from a WCF Service
    /// </summary>
    public class SubscriptionLeaseProvider: ISubscriptionLeaseProvider
    {
        private readonly string subscriptionUrl;

        /// <summary>
        /// Creates a new subscription lease provider that connects to the specified WCF service
        /// </summary>
        /// <param name="subscriptionUrl">URL to WCF provider for ISubscriptionLicensingService</param>
        public SubscriptionLeaseProvider(string subscriptionUrl)
        {
            if (string.IsNullOrEmpty(subscriptionUrl))
                throw new ArgumentException(nameof(subscriptionUrl));

            this.subscriptionUrl = subscriptionUrl;
        }
        
        /// <summary>
        /// Gets a new subscription lease from the configured WCF service
        /// </summary>
        /// <param name="originalLicense"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public string GetLeaseSubscription(string originalLicense)
        {
            var service =
                ChannelFactory<ISubscriptionLicensingService>.CreateChannel(
                    new BasicHttpBinding(), new EndpointAddress(subscriptionUrl));

            try
            {
                return service.LeaseLicense(originalLicense);
            }
            finally
            {
                var communicationObject = service as ICommunicationObject;
                if (communicationObject != null)
                {
                    try
                    {
                        communicationObject.Close(TimeSpan.FromMilliseconds(200));
                    }
                    catch
                    {
                        communicationObject.Abort();
                    }
                }
            }
        }
    }
}
