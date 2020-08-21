using Caliburn.PresentationFramework.ApplicationModel;
using NSubstitute;
using Rhino.Licensing.AdminTool.Factories;
using Rhino.Licensing.AdminTool.Services;
using Rhino.Licensing.AdminTool.ViewModels;
using Xunit;

namespace Rhino.Licensing.AdminTool.Tests.Services
{
    public class StatusServiceTests
    {
        private readonly IStatusService _statusService;

        public StatusServiceTests()
        {
            var windowManager    = Substitute.For<IWindowManager>();
            var viewModelFactory = Substitute.For<IViewModelFactory>();
            var projectService   = Substitute.For<IProjectService>();

            _statusService = new ShellViewModel(windowManager, viewModelFactory, projectService);
        }

        [Fact]
        public void Can_Update_Status_Message()
        {
            _statusService.Update("Issued Licenses: {0}", 10);

            var vm = _statusService as IShellViewModel;

            Assert.Equal("Issued Licenses: 10", vm?.StatusMessage);
        }
    }
}