using System;
using System.IO;
using Serilog;

namespace Rhino.Licensing
{
    /// <summary>
    /// License validator validates a license file
    /// that can be located on disk.
    /// </summary>
    public class LicenseValidator : AbstractLicenseValidator
    {
        private readonly string licensePath;
        private string inMemoryLicense;

        /// <summary>
        /// Creates a new instance of <seealso cref="LicenseValidator"/>.
        /// </summary>
        /// <param name="publicKey">public key</param>
        /// <param name="licensePath">path to license file</param>
        public LicenseValidator(string publicKey, string licensePath)
            : base(publicKey)
        {
            this.licensePath = licensePath;
        }

        /// <summary>
        /// Gets or Sets the license content
        /// </summary>
        protected override string License
        {
            get => inMemoryLicense ?? File.ReadAllText(licensePath);
            set
            {
                try
                {
                    File.WriteAllText(licensePath, value);
                }
                catch (Exception e)
                {
                    inMemoryLicense = value;
                    Log.Warning("Could not write new license value, using in memory model instead", e);
                }
            }
        }

        /// <summary>
        /// Validates loaded license
        /// </summary>
        public override void AssertValidLicense()
        {
            if (File.Exists(licensePath) == false)
            {
                Log.Warning("Could not find license file: {0}", licensePath);
                throw new LicenseFileNotFoundException();
            }

            base.AssertValidLicense();
        }

        /// <summary>
        /// Removes existing license from the machine.
        /// </summary>
        public override void RemoveExistingLicense()
        {
            File.Delete(licensePath);
        }
    }
}
