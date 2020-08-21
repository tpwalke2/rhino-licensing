using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Caliburn.PresentationFramework;
using Rhino.Licensing.Contracts;

namespace Rhino.Licensing.AdminTool.Model
{
    [DataContract(Name = "License", Namespace = "http://schemas.hibernatingrhinos.com/")]
    public class License : PropertyChangedBase
    {
        private ObservableCollection<UserData> _data;
        private string _ownerName;
        private DateTime? _expirationDate;
        private LicenseType _licenseType;
        private Guid _id;

        public License()
        {
            _id = Guid.NewGuid();
            Data = new ObservableCollection<UserData>();
        }

        [DataMember]
        public virtual Guid ID
        {
            get => _id;
            set
            {
                _id = value;
                NotifyOfPropertyChange(() => ID);
            }
        }

        [DataMember]
        public virtual string OwnerName
        {
            get => _ownerName;
            set
            {
                _ownerName = value;
                NotifyOfPropertyChange(() => OwnerName);
            }
        }

        [DataMember]
        public virtual DateTime? ExpirationDate
        {
            get => _expirationDate;
            set
            {
                _expirationDate = value;
                NotifyOfPropertyChange(() => ExpirationDate);
            }
        }

        [DataMember]
        public virtual LicenseType LicenseType
        {
            get => _licenseType;
            set
            {
                _licenseType = value;
                NotifyOfPropertyChange(() => LicenseType);
            }
        }

        [DataMember]
        public virtual ObservableCollection<UserData> Data
        {
            get => _data;
            private set
            {
                _data = value;
                NotifyOfPropertyChange(() => Data);
            }
        }
    }
}