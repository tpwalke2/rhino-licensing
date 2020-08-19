using System;
using System.Collections.Generic;

namespace Rhino.Licensing.Contracts
{
    public interface ILicenseValidator
    {
        string Name { get; }
        Guid UserId { get; }
        IDictionary<string, string> LicenseAttributes { get; }
        void AssertValidLicense();
        LicenseType LicenseType { get; }
    }
}
