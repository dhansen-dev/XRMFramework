using NUnit.Framework;

using XRMFramework.Container;

namespace XRMFramework.Tests.Container
{
    [TestFixture(Category = nameof(SuperSimpleCRMInjector))]
    public class MapTests : ContainerTestsBase
    {
        [Test]
        public void Map_InterfaceMappedWithConcreteType_ReturnsInstanceTest()
        {
            Injector
                .Map<IMyService, MyService>();

            var MyService = Injector.GetService<IMyService>();

            Assert.NotNull(MyService);
        }

        [Test]
        public void Map_TypeMappedAsSingleton_ReturnsSameInstanceOnEachCall()
        {
            Injector
                .Map<IMyService, MyService>();

            var MyService1 = Injector.GetService<IMyService>();
            var MyService2 = Injector.GetService<IMyService>();

            Assert.True(MyService1 == MyService2);
        }

        [Test]
        public void Map_TypeMappedAsTransient_ReturnsNewInstanceOnEachCall()
        {
            Injector
                .Map<IMyService, MyService>(SuperSimpleCRMInjector.Scope.Transient);

            var MyService1 = Injector.GetService<IMyService>();
            var MyService2 = Injector.GetService<IMyService>();

            Assert.False(MyService1 == MyService2);
        }

        [Test]
        public void Map_TypeMappedWithDefaultScope_ReturnsSameInstanceOnEachCall()
        {
            Injector
                .Map<IMyService, MyService>();

            var MyService1 = Injector.GetService<IMyService>();
            var MyService2 = Injector.GetService<IMyService>();

            Assert.True(MyService1 == MyService2);
        }

        #region Test Classes

        internal class MyService : IMyService
        {
            public void ServiceCall()
            {
            }
        }

        internal interface IMyService
        {
            void ServiceCall();
        }

        #endregion Test Classes
    }
}