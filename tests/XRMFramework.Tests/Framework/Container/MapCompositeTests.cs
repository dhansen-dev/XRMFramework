using FluentAssertions;

using NUnit.Framework;

using System.Collections.Generic;
using System.Linq;

using XRMFramework.Container;

namespace XRMFramework.Tests.Container
{
    [TestFixture(Category = nameof(SuperSimpleCRMInjector))]
    public class MapCompositeTests : ContainerTestsBase
    {
        [Test]
        public void Map_ImplementationIsComposite_CompositeTypeIsReturned()
        {
            Injector.Map(typeof(IValidate), typeof(CompositeValidator));

            var composite = Injector.GetService<IValidate>();

            composite.Validate().Should().BeTrue("because all validators returns true");
        }

        [Test]
        public void Map_ImplementationIsGenericComposite_CompositeTypeIsReturned()
        {
            Injector.Map(typeof(ICompositeValidate<>), typeof(CompositeValidatorWithGenericInterface));

            var composite = Injector.GetService<ICompositeValidate<EntityToValidate>>();

            composite.Validate(null).Should().BeTrue("because all validators returns true");
        }

        #region Test Classes

        #region Composite validator with generic interface classes

        internal class CompositeValidatorWithGenericInterface : ICompositeValidate<EntityToValidate>
        {
            private readonly IEnumerable<ICompositeValidate<EntityToValidate>> _validators;

            public CompositeValidatorWithGenericInterface(IEnumerable<ICompositeValidate<EntityToValidate>> validators)
            {
                _validators = validators;
            }

            public bool Validate(EntityToValidate obj)
                => _validators.All(t => t.Validate(obj));
        }

        internal interface ICompositeValidate<TEntityToValidate>
        {
            bool Validate(TEntityToValidate obj);
        }

        #endregion Composite validator with generic interface classes

        #region Composite validator classes

        public class CompositeValidator : IValidate
        {
            public IEnumerable<IValidate> Validators { get; }

            public CompositeValidator(IEnumerable<IValidate> validators)
            {
                Validators = validators;
            }

            public bool Validate()
                => Validators.All(t => t.Validate());
        }

        public class Validator1 : IValidate
        {
            public bool Validate()
                => true;
        }

        #endregion Composite validator classes

        internal class EntityToValidate
        {
        }

        public interface IValidate
        {
            bool Validate();
        }

        #endregion Test Classes
    }
}