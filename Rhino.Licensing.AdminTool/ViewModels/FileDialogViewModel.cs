using System.Collections.Generic;
using Caliburn.PresentationFramework.Screens;

namespace Rhino.Licensing.AdminTool.ViewModels
{
    public abstract class FileDialogViewModel : Screen, IFileDialogViewModel
    {
        private bool _addExtension;
        private bool _checkFileExists;
        private bool _checkPathExists;
        private string _defaultExtension;
        private string _fileName;
        private IEnumerable<string > _fileNames;
        private string _filter;
        private string _initialDirectory;
        private string _title;
        private bool? _result;

        public virtual bool AddExtension
        {
            get => _addExtension;
            set
            {
                _addExtension = value;
                NotifyOfPropertyChange(() => AddExtension);
            }
        }

        public virtual bool CheckFileExists
        {
            get => _checkFileExists;
            set
            {
                _checkFileExists = value;
                NotifyOfPropertyChange(() => CheckFileExists);
            }
        }

        public virtual bool CheckPathExists
        {
            get => _checkPathExists;
            set
            {
                _checkPathExists = value;
                NotifyOfPropertyChange(() => CheckPathExists);
            }
        }

        public virtual string DefaultExtension
        {
            get => _defaultExtension;
            set
            {
                _defaultExtension = value;
                NotifyOfPropertyChange(() => DefaultExtension);
            }
        }

        public virtual string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                NotifyOfPropertyChange(() => FileName);
            }
        }

        public virtual IEnumerable<string> FileNames
        {
            get => _fileNames;
            set
            {
                _fileNames = value;
                NotifyOfPropertyChange(() => FileNames);
            }
        }

        public virtual bool? Result
        {
            get => _result;
            set
            {
                _result = value;
                NotifyOfPropertyChange(() => Result);
            }
        }

        public virtual string Filter
        {
            get => _filter;
            set
            {
                _filter = value;
                NotifyOfPropertyChange(() => Filter);
            }
        }

        public virtual string InitialDirectory
        {
            get => _initialDirectory;
            set
            {
                _initialDirectory = value;
                NotifyOfPropertyChange(() => InitialDirectory);
            }
        }

        public virtual string Title
        {
            get => _title;
            set
            {
                _title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }
    }
}