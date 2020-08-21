using Castle.Windsor;
using NSubstitute;
using Rhino.Licensing.AdminTool.Startup;
using Xunit;

namespace Rhino.Licensing.AdminTool.Tests.Startup
{
    public class InstallersTests
    {
        private readonly IWindsorContainer _container;
        private readonly GuyWire _guyWire;

        public InstallersTests()
        {
            _container = new WindsorContainer();

            _guyWire = Substitute.ForPartsOf<GuyWire>(_container);
            _guyWire.Container.Returns(_container);
            _guyWire.Wire();
        }

        [Fact]
        public void GuyWire_Delegates_To_FactoryInstaller()
        {
            var installer = new FactoryRegistration();

            _guyWire.ComponentsInfo.Returns(new[] { installer });
            
            var ex = Record.Exception(() => _guyWire.Wire());
            Assert.Null(ex);
        }

        [Fact]
        public void GuyWire_Delegates_To_ServiceInstaller()
        {
            var installer = new ServiceRegistration();

            _guyWire.ComponentsInfo.Returns(new[] { installer });

            var ex = Record.Exception(() => _guyWire.Wire());
            Assert.Null(ex);
        }

        [Fact]
        public void GuyWire_Delegates_To_ViewModelInstaller()
        {
            var installer = new ViewModelRegistration();

            _guyWire.ComponentsInfo.Returns(new[] { installer });

            var ex = Record.Exception(() => _guyWire.Wire());
            Assert.Null(ex);
        }

        [Fact]
        public void GuyWire_Delegates_To_ViewInstaller()
        {
            var installer = new ViewRegistration();

            _guyWire.ComponentsInfo.Returns(new[] { installer });

            var ex = Record.Exception(() => _guyWire.Wire());
            Assert.Null(ex);
        }
    }
}