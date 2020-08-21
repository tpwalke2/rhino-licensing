using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using Caliburn.PresentationFramework;

namespace Rhino.Licensing.AdminTool.Model
{
    [DataContract(Name = "Product", Namespace = "http://schemas.hibernatingrhinos.com/")]
    public class Product : PropertyChangedBase
    {
        private readonly RSA _key;
        private string _publicKey;
        private string _name;
        private string _privateKey;
        private Guid _id;
        private ObservableCollection<License> _issuedLicenses;

        public Product()
        {
            _key = RSA.Create();
            _id = Guid.NewGuid();
            _issuedLicenses = new ObservableCollection<License>();
            _publicKey = _key.ToXmlString(false);
            _privateKey = _key.ToXmlString(true);
        }

        [DataMember]
        public virtual Guid Id
        {
            get => _id;
            set
            {
                _id = value;
                NotifyOfPropertyChange(() => Id);
            }
        }

        [DataMember]
        public virtual string Name
        {
            get => _name;
            set
            {
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        [DataMember]
        public virtual string PrivateKey
        {
            get => _privateKey;
            set
            {
                _privateKey = value;
                NotifyOfPropertyChange(() => PrivateKey);
            }
        }

        [DataMember]
        public virtual string PublicKey
        {
            get => _publicKey;
            set
            {
                _publicKey = value;
                NotifyOfPropertyChange(() => PublicKey);
            }
        }

        [DataMember]
        public virtual ObservableCollection<License> IssuedLicenses
        {
            get => _issuedLicenses;
            private set
            {
                _issuedLicenses = value;
                NotifyOfPropertyChange(() => IssuedLicenses);
            }
        }
    }
}