using NSubstitute;
using Rhino.Licensing.AdminTool.Factories;
using Rhino.Licensing.AdminTool.Services;
using Rhino.Licensing.AdminTool.ViewModels;
using Xunit;
using OpenFileDialog = Rhino.Licensing.AdminTool.Dialogs.OpenFileDialog;
using SaveFileDialog = Rhino.Licensing.AdminTool.Dialogs.SaveFileDialog;

namespace Rhino.Licensing.AdminTool.Tests.Services
{
    public class DialogServiceTests
    {
        [Fact]
        public void Can_Show_OpenFileDialog()
        {
            var model = CreateOpenFileDialogModel(true);
            var factory = Substitute.For<IDialogFactory>();
            var dialog = Substitute.For<OpenFileDialog>();

            factory.Create<OpenFileDialog, IOpenFileDialogViewModel>(model).Returns(dialog);
            dialog.ViewModel.Returns(model);

            new DialogService(factory).ShowOpenFileDialog(model);

            dialog.Received(1).ShowDialog();
        }

        [Fact]
        public void Can_Show_SaveFileDialog()
        {
            var model   = CreateSaveFileDialogModel(true);
            var factory = Substitute.For<IDialogFactory>();
            var dialog  = Substitute.For<SaveFileDialog>();

            var service = new DialogService(factory) as IDialogService;

            factory.Create<SaveFileDialog, ISaveFileDialogViewModel>(model).Returns(dialog);

            service.ShowSaveFileDialog(model);

            dialog.Received(1).ShowDialog();
        }

        private IOpenFileDialogViewModel CreateOpenFileDialogModel(bool? result)
        {
            return new OpenFileDialogViewModel
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExtension = "rlic",
                Filter = "Rhino License Project|*.rlic",
                InitialDirectory = "C:\\",
                MultiSelect = false,
                Title = "Open File Dialog",
                Result = result
            };
        }

        private ISaveFileDialogViewModel CreateSaveFileDialogModel(bool? result)
        {
            return new SaveFileDialogViewModel
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExtension = "rlic",
                Filter = "Rhino License Project|*.rlic",
                InitialDirectory = "C:\\",
                Title = "Open File Dialog",
                Result = result
            };
        }

    }
}