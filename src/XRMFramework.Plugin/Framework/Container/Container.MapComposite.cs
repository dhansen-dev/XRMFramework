using System;
using System.Linq;

namespace XRMFramework.Container
{
    public partial class SuperSimpleCRMInjector
    {
        /// <summary>
        /// Maps a composite type
        /// </summary>
        /// <param name="abstraction">The interface to map</param>
        /// <param name="implementation">The composite type to map</param>
        /// <param name="scope">Scope to use when resolving an instance</param>
        /// <returns>this</returns>
        public SuperSimpleCRMInjector MapComposite(Type abstraction, Type implementation, Scope scope = Scope.Singleton)
        {
            var types = GetTypesImplementingAbstraction(abstraction);

            Type implementedInterfaceOfAbstraction = abstraction;

            if (abstraction.IsGenericType)
            {
                implementedInterfaceOfAbstraction = types.Single(type => type.ConcreteType == implementation).InterfaceType;
            }

            var mapping = new Mapping
            {
                ConcreteType = implementation,
                InterfaceType = implementedInterfaceOfAbstraction,
                Scope = scope,
                ChildMapping = types.Where(t => t.ConcreteType != implementation).Select(type => new Mapping
                {
                    ConcreteType = type.ConcreteType,
                    InterfaceType = type.InterfaceType
                }).ToList()
            };

            mappings.Add(implementedInterfaceOfAbstraction, mapping);

            return this;
        }
    }
}