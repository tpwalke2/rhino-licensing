using Caliburn.PresentationFramework.Filters;
using Caliburn.PresentationFramework.Screens;
using Rhino.Licensing.AdminTool.Model;

namespace Rhino.Licensing.AdminTool.ViewModels
{
    public class UserDataViewModel : Screen, IUserDataViewModel
    {
        private License _currentLicense;
        private UserData _selectedKeyValue;
        private string _currentKey;
        private string _currentValue;

        public virtual License CurrentLicense
        {
            get => _currentLicense;
            set
            {
                _currentLicense = value;
                NotifyOfPropertyChange(() => CurrentLicense);
            }
        }

        public virtual UserData SelectedKeyValue
        {
            get => _selectedKeyValue;
            set
            {
                _selectedKeyValue = value;
                NotifyOfPropertyChange(() => SelectedKeyValue);
            }
        }

        public virtual string CurrentKey
        {
            get => _currentKey;
            set
            {
                _currentKey = value;
                NotifyOfPropertyChange(() => CurrentKey);
            }
        }

        public virtual string CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;
                NotifyOfPropertyChange(() => CurrentValue);
            }
        }

        public virtual bool CanRemoveSelected => CurrentLicense != null && SelectedKeyValue != null;

        public virtual bool CanAddKey =>
            CurrentLicense != null && 
            !string.IsNullOrWhiteSpace(CurrentKey) &&
            !string.IsNullOrWhiteSpace(CurrentValue);

        [AutoCheckAvailability]
        public virtual void RemoveSelected()
        {
            if (CurrentLicense.Data.Contains(SelectedKeyValue))
            {
                CurrentLicense.Data.Remove(SelectedKeyValue);
            }
        }

        [AutoCheckAvailability]
        public virtual void AddKey()
        {
            CurrentLicense.Data.Add(new UserData { Key = CurrentKey, Value = CurrentValue });
        }
    }
}