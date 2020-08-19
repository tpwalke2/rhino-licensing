using Rhino.Licensing.Contracts;

namespace Rhino.Licensing
{
    /// <summary>
    /// Creates a factory for creating license validators
    /// </summary>
    public class LicenseValidatorFactory : ILicenseValidatorFactory
    {
        /// <summary>
        /// Creates a LicenseValidator instance
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="licensePath"></param>
        /// <param name="allowFloatingLicenses"></param>
        /// <returns></returns>
        public ILicenseValidator Create(string publicKey, string licensePath, bool allowFloatingLicenses)
        {
            return new LicenseValidator(publicKey, licensePath)
            {
                DisableFloatingLicenses = allowFloatingLicenses
            };
        }
    }
}
