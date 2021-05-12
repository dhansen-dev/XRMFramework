using System;
using System.Linq;

namespace XRMFramework.Container
{
    public partial class SuperSimpleCRMInjector
    {
        public SuperSimpleCRMInjector MapDecorator<TInterface, TTypeToDecorate>(params Type[] decorators) where TTypeToDecorate : TInterface
            => MapDecorator(typeof(TInterface), typeof(TTypeToDecorate), decorators);

        public SuperSimpleCRMInjector MapDecorator(Type abstraction, Type coreImplementation, params Type[] decorators)
        {
            mappings.Add(abstraction, new Mapping
            {
                ConcreteType = coreImplementation,
                InterfaceType = abstraction,
                IsDecorated = true,
            });

            var mapping = mappings[abstraction];

            mapping.ChildMapping.AddRange(decorators.Select(decorator => new Mapping
            {
                InterfaceType = abstraction,
                ConcreteType = decorator
            }));

            return this;
        }
    }
}