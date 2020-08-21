using Caliburn.PresentationFramework.ApplicationModel;
using Caliburn.PresentationFramework.Screens;
using NSubstitute;
using Rhino.Licensing.AdminTool.Factories;
using Rhino.Licensing.AdminTool.Services;
using Rhino.Licensing.AdminTool.Tests.Base;
using Rhino.Licensing.AdminTool.ViewModels;
using Xunit;

namespace Rhino.Licensing.AdminTool.Tests.ViewModels
{
    public class ShellViewModelTests : TestBase
    {
        private readonly IViewModelFactory _viewModelFactory;
        private readonly IWindowManager _windowManager;
        private readonly IProjectService _projectService;
        private readonly IDialogService _dialogService;
        private readonly IStatusService _statusService;
        private readonly ProjectViewModel _projectViewModel;
        private readonly IExportService _exportService;

        public ShellViewModelTests()
        {
            _viewModelFactory = Substitute.For<IViewModelFactory>();
            _windowManager = Substitute.For<IWindowManager>();
            _projectService = Substitute.For<IProjectService>();
            _dialogService = Substitute.For<IDialogService>();
            _statusService = Substitute.For<IStatusService>();
            _exportService = Substitute.For<IExportService>();
            _projectViewModel = Substitute.For<ProjectViewModel>(_projectService, _dialogService, _statusService, _exportService, _viewModelFactory, _windowManager);
        }

        [Fact]
        public void ShowAbout_Action_Displays_Dialog()
        {
            var shell = CreateShellViewModel();
            var aboutVm = new AboutViewModel();

            _viewModelFactory.Create<AboutViewModel>().Returns(aboutVm);

            shell.ShowAboutDialog();

            _windowManager.Received().ShowDialog(Arg.Is(aboutVm), Arg.Is<object>(null));
        }

        [Fact]
        public void CreateNewProject_Opens_ProjectViewModel()
        {
            var shell = CreateShellViewModel();
            var vm = CreateProjectViewModel();

            _viewModelFactory.Create<ProjectViewModel>().Returns(vm);

            shell.CreateNewProject();

            Assert.NotNull(shell.ActiveItem);
            Assert.Same(vm, shell.ActiveItem);
        }

        [Fact]
        public void OpenProject_Opens_ProjectViewModel_When_User_Accepts_The_Open_Dialog()
        {
            var shell = CreateShellViewModel();

            _projectViewModel.Open().Returns(true);
            _viewModelFactory.Create<ProjectViewModel>().Returns(_projectViewModel);

            shell.OpenProject();

            Assert.NotNull(shell.ActiveItem);
            Assert.Same(_projectViewModel, shell.ActiveItem);
        }

        [Fact]
        public void OpenProject_Wont_Show_ProjectViewModel_When_User_Cancels_The_Open_Dialog()
        {
            var shell = CreateShellViewModel();

            _projectViewModel.Open().Returns(false);
            _viewModelFactory.Create<ProjectViewModel>().Returns(_projectViewModel);

            shell.OpenProject();

            Assert.Null(shell.ActiveItem);
        }

        [Fact]
        public void OpenProject_Calls_Open_On_ProjectViewModel()
        {
            var shell = CreateShellViewModel();

            _viewModelFactory.Create<ProjectViewModel>().Returns(_projectViewModel);

            shell.OpenProject();

            _projectViewModel.Received().Open();
        }

        [Fact]
        public void Opening_New_Screen_Will_Destroy_Previous()
        {
            var shell = CreateShellViewModel();
            var vm = CreateProjectViewModel();

            shell.ActiveItem = vm; //Set initial view

            shell.ActiveItem = new Screen(); //Change to new view

            _viewModelFactory.Received().Release(Arg.Is(vm));
        }

        private ProjectViewModel CreateProjectViewModel()
        {
            return new ProjectViewModel(_projectService, _dialogService, _statusService, _exportService, _viewModelFactory, _windowManager);
        }

        private ShellViewModel CreateShellViewModel()
        {
            return new ShellViewModel(_windowManager, _viewModelFactory, _projectService);
        }
    }
}