using NUnit.Framework;

using XRMFramework.Container;

namespace XRMFramework.Tests.Container
{
    public abstract class ContainerTestsBase
    {
        protected SuperSimpleCRMInjector Injector { get; private set; }

        [SetUp]
        public void Setup()
        {
            Injector = new SuperSimpleCRMInjector();
        }
    }
}