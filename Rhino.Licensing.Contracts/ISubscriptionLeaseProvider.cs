namespace Rhino.Licensing.Contracts
{
    public interface ISubscriptionLeaseProvider
    {
        string GetLeaseSubscription(string originalLicense);
    }
}
