using System.IO;
using System.Windows;
using Caliburn.PresentationFramework.ApplicationModel;
using Rhino.Licensing.AdminTool.Factories;
using Rhino.Licensing.AdminTool.Model;
using Rhino.Licensing.AdminTool.Services;
using Rhino.Licensing.AdminTool.Tests.Base;
using Rhino.Licensing.AdminTool.ViewModels;
using Xunit;
using Caliburn.Testability.Extensions;
using NSubstitute;
using Rhino.Licensing.Contracts;

namespace Rhino.Licensing.AdminTool.Tests.ViewModels
{
    public class ProjectViewModelTests : TestBase
    {
        private readonly IDialogService _dialogService;
        private readonly IProjectService _projectService;
        private readonly IStatusService _statusService;
        private readonly IWindowManager _windowManager;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly IExportService _exportService;

        public ProjectViewModelTests()
        {
            _dialogService = Substitute.For<IDialogService>();
            _projectService = Substitute.For<IProjectService>();
            _statusService = Substitute.For<IStatusService>();
            _windowManager = Substitute.For<IWindowManager>();
            _viewModelFactory = Substitute.For<IViewModelFactory>();
            _exportService = Substitute.For<IExportService>();
        }

        [Fact]
        public void Creating_New_ProductViewModel_Will_Have_Empty_Product()
        {
            var vm = CreateProjectViewModel();
            
            Assert.Null(vm.CurrentProject);
        }

        [Fact]
        public void Fires_PropertyChange_Notification()
        {
            var vm = CreateProjectViewModel();

            vm.AssertThatProperty(x => x.CurrentProject).RaisesChangeNotification();
        }

        [Fact]
        public void Can_Not_Save_If_Name_Is_Not_Provided()
        {
            var vm = CreateProjectViewModel();

            vm.CurrentProject = new Project
            {
                Product = new Product
                {
                    Name = null
                }
            };

            Assert.False(vm.CanSave());
        }

        [Fact]
        public void Can_Save_If_Name_Is_Provided()
        {
            var vm = CreateProjectViewModel();
            
            vm.CurrentProject = new Project
            {
                Product = new Product
                {
                    Name = "New Product"
                }
            };

            Assert.True(vm.CanSave());
        }

        [Fact]
        public void Save_Action_Will_Open_SaveDialog()
        {
            var dialogModel = new SaveFileDialogViewModel {Result = true};
            _viewModelFactory.Create<ISaveFileDialogViewModel>().Returns(dialogModel);

            var viewModel = CreateProjectViewModel();
            viewModel.Save();

            _dialogService.Received().ShowSaveFileDialog(Arg.Is(dialogModel));
        }

        [Fact]
        public void Save_Action_Will_Call_ProjectService_If_Proper_Result_Is_Set()
        {
            var existingFile = Path.GetTempFileName();
            var choosenFile = new FileInfo(existingFile);
            var dialogViewModel = new SaveFileDialogViewModel {Result = true, FileName = existingFile};

            _viewModelFactory.Create<ISaveFileDialogViewModel>().Returns(dialogViewModel);
            
            var viewModel = CreateProjectViewModel();
            viewModel.Save();

            _projectService.Received().Save(Arg.Any<Project>(), Arg.Is(choosenFile));
            _dialogService.Received().ShowSaveFileDialog(dialogViewModel);
        }

        [Fact]
        public void Will_Not_Proceed_To_Save_When_No_File_Is_Selected()
        {
            var dialogModel = new SaveFileDialogViewModel {Result = true, FileName = null};

            _viewModelFactory.Create<ISaveFileDialogViewModel>().Returns(dialogModel);
            
            var viewModel = CreateProjectViewModel();
            viewModel.Save();
            
            _projectService.DidNotReceive().Save(Arg.Any<Project>(), Arg.Any<FileInfo>());
            _dialogService.Received().ShowSaveFileDialog(dialogModel);
        }

        [Fact]
        public void Key_Pair_Is_Generated_When_A_Product_Is_Created()
        {
            var vm = CreateProjectViewModel();

            vm.CurrentProject = new Project {Product = new Product()};

            Assert.NotNull(vm.CurrentProject.Product.PublicKey);
            Assert.NotNull(vm.CurrentProject.Product.PrivateKey);
            Assert.Contains("<P>", vm.CurrentProject.Product.PrivateKey); //Makes sure it is only private
            Assert.Contains("<Modulus>", vm.CurrentProject.Product.PublicKey); //Makes sure it is public
        }

        [Fact]
        public void Default_Project_Save_Dialog_Has_Correct_Filter()
        {
            var dialogViewModel = new SaveFileDialogViewModel { Result = true };
            _viewModelFactory.Create<ISaveFileDialogViewModel>().Returns(dialogViewModel);

            var vm = CreateProjectViewModel();
            vm.Save();

            Assert.Equal("Rhino Project|*.rlic", dialogViewModel.Filter);
        }

        [Fact]
        public void Default_Project_Open_Dialog_Has_Correct_Filter()
        {
            var dialogViewModel = new OpenFileDialogViewModel { Result = true };
            _viewModelFactory.Create<IOpenFileDialogViewModel>().Returns(dialogViewModel);

            var vm = CreateProjectViewModel();
            vm.Open();

            Assert.Equal("Rhino Project|*.rlic", dialogViewModel.Filter);
        }

        [Fact]
        public void Can_Copy_Keys_To_Clipboard()
        {
            var keyContent = "Key Content";
            var vm = CreateProjectViewModel();

            vm.CopyToClipboard(keyContent);

            var readback = Clipboard.GetText(TextDataFormat.UnicodeText);

            Assert.Equal(keyContent, readback);
        }

        [Fact]
        public void Calling_Save_For_Second_Time_Wont_Show_SaveDialog()
        {
            var dialogModel = new SaveFileDialogViewModel {Result = true, FileName = "C:\\"};
            _viewModelFactory.Create<ISaveFileDialogViewModel>().Returns(dialogModel);

            var vm = CreateProjectViewModel();
            
            vm.Save(); //For the first time, opens the dialog

            vm.Save(); //For the second time overwrites the same file

            _dialogService.Received().ShowSaveFileDialog(Arg.Is(dialogModel));
        }

        [Fact]
        public void Open_Shows_OpenDialog()
        {
            var dialogModel = new OpenFileDialogViewModel { Result = true, FileName = "C:\\" };
            _viewModelFactory.Create<IOpenFileDialogViewModel>().Returns(dialogModel);

            var vm = CreateProjectViewModel();
            vm.Open();

            _dialogService.Received().ShowOpenFileDialog(Arg.Is(dialogModel));
        }

        [Fact]
        public void Open_Returns_False_When_User_Cancels_The_Open_Dialog()
        {
            var dialogModel = new OpenFileDialogViewModel { Result = false, FileName = "C:\\" };
            _viewModelFactory.Create<IOpenFileDialogViewModel>().Returns(dialogModel);

            var vm = CreateProjectViewModel();
            var opened = vm.Open();

            Assert.False(opened);
        }

        [Fact]
        public void Open_Loads_Project()
        {
            var p = new Project();
            _projectService.Open(Arg.Any<FileInfo>()).Returns(p);

            var dialogModel = new OpenFileDialogViewModel { Result = true, FileName = "C:\\" };
            _viewModelFactory.Create<IOpenFileDialogViewModel>().Returns(dialogModel);

            var vm = CreateProjectViewModel();
            vm.Open();

            Assert.NotNull(vm.CurrentProject);
            Assert.Same(p, vm.CurrentProject);
        }

        [Fact]
        public void Can_Issue_New_License_When_Keys_Are_Generated()
        {
            var vm = CreateProjectViewModel();

            vm.CurrentProject = new Project();
            var canAdd = vm.CanAddLicense();

            Assert.True(canAdd);
        }

        [Fact]
        public void Can_Issues_New_License()
        {
            var vm = CreateProjectViewModel();
            var issueVm = new IssueLicenseViewModel(new UserDataViewModel(), new LicenseInfoViewModel());

            _viewModelFactory.Create<IssueLicenseViewModel>().Returns(issueVm);
            _windowManager.ShowDialog(Arg.Is(issueVm), Arg.Any<object>()).Returns(true);

            vm.CurrentProject = new Project { Product = new Product() };
            vm.AddLicense();

            Assert.Single(vm.CurrentProject.Product.IssuedLicenses);
        }

        [Fact]
        public void Can_Export_License_When_Selected()
        {
            var vm = CreateProjectViewModel();

            vm.SelectedLicense = null;

            var canExportWhenNull = vm.CanExportLicense();

            vm.SelectedLicense = new License();

            var canExportWhenNotNull = vm.CanExportLicense();

            Assert.False(canExportWhenNull);
            Assert.True(canExportWhenNotNull);
        }

        [Fact]
        public void Export_Selected_License()
        {
            var licensepath = @"c:\License.xml";
            var vm = CreateProjectViewModel();
            var saveDialog = new SaveFileDialogViewModel { Result = true, FileName = licensepath };

            vm.CurrentProject = new Project {Product = new Product()};
            vm.SelectedLicense = new License { OwnerName = "John Doe", ExpirationDate = null, LicenseType = LicenseType.Trial };

            _viewModelFactory.Create<ISaveFileDialogViewModel>().Returns(saveDialog);

            vm.ExportLicense();

            _exportService.Received().Export(Arg.Is(vm.CurrentProject.Product), Arg.Is(vm.SelectedLicense), Arg.Any<FileInfo>());
        }

        private ProjectViewModel CreateProjectViewModel()
        {
            return new ProjectViewModel(_projectService, _dialogService, _statusService, _exportService, _viewModelFactory, _windowManager);
        }
    }
}