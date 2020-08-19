namespace Rhino.Licensing.Contracts
{
    public interface ILicenseValidatorFactory
    {
        ILicenseValidator Create(string publicKey, string licensePath, bool allowFloatingLicenses);
    }
}
