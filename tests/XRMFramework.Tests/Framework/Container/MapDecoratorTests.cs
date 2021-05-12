using FluentAssertions;

using NUnit.Framework;

using System;

using XRMFramework.Container;

namespace XRMFramework.Tests.Container
{
    [TestFixture(Category = nameof(SuperSimpleCRMInjector))]
    public class MapDecoratorTests : ContainerTestsBase
    {
        [Test]
        public void Map_MapDecorator_DecoratedTypeIsReturned()
        {
            Injector.MapDecorator<IDecoratorTest, NonSensitiveLogger>(typeof(SecureLoggerDecorator));

            var decorator = Injector.GetService<IDecoratorTest>();

            decorator
                .Should()
                .BeOfType<SecureLoggerDecorator>("because the returned type should be the last in the list of decorators");
        }

        #region Test Classes

        public class NonSensitiveLogger : IDecoratorTest
        {
            public void Log(string message)
            {
                Console.Write(message);
            }
        }

        public class SecureLoggerDecorator : IDecoratorTest
        {
            private readonly IDecoratorTest _decoratee;

            public SecureLoggerDecorator(IDecoratorTest decoratee)
            {
                _decoratee = decoratee;
            }

            public void Log(string message)
            {
                if (message.StartsWith("SECURE"))
                {
                    _decoratee.Log(message);
                }
                else
                {
                    throw new ArgumentException(nameof(message));
                }
            }
        }

        public interface IDecoratorTest
        {
            void Log(string message);
        }

        #endregion Test Classes
    }
}