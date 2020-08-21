namespace Rhino.Licensing.AdminTool.ViewModels
{
    public interface IOpenFileDialogViewModel : IFileDialogViewModel
    {
        bool MultiSelect { get; set; }
    }

    public class OpenFileDialogViewModel : FileDialogViewModel, IOpenFileDialogViewModel
    {
        private bool _multiSelect;

        public virtual bool MultiSelect
        {
            get => _multiSelect;
            set
            {
                _multiSelect = value;
                NotifyOfPropertyChange(() => MultiSelect);
            }
        }
    }
}