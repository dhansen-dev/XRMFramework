using System;
using System.Collections.Generic;
using System.Linq;

namespace XRMFramework.Container
{
    public partial class SuperSimpleCRMInjector
    {
        private SuperSimpleCRMInjector MapGenericType(Type abstraction, Type implementation, Scope scope = Scope.Singleton)
        {
            var foundAbstractions
            = GetTypesImplementingAbstraction(abstraction);

            var closedAbstractionsGroupings = foundAbstractions
                                .Where(t => !t.InterfaceType.ContainsGenericParameters)
                                .GroupBy(t => t.InterfaceType.GenericTypeArguments, t => new
                                {
                                    t.ConcreteType,
                                    t.InterfaceType
                                });

            var genericTypeDefinitionOfAbstraction = abstraction.GetGenericTypeDefinition();

            foreach (var closedAbstractionGroup in closedAbstractionsGroupings)
            {
                var closedAbstraction = closedAbstractionGroup.First();

                Type constructedType = implementation;

                if (implementation.IsGenericType)
                {
                    var genericTypeDefOfImplementation = implementation.GetGenericTypeDefinition();
                    constructedType = genericTypeDefOfImplementation.MakeGenericType(closedAbstraction.InterfaceType.GenericTypeArguments);
                }

                var parentMap = new Mapping
                {
                    ConcreteType = implementation,
                    InterfaceType = genericTypeDefinitionOfAbstraction.MakeGenericType(closedAbstractionGroup.Key),
                    Scope = scope,
                    ChildMapping = new List<Mapping>()
                };

                mappings.Add(parentMap.InterfaceType, parentMap);
            }

            return this;
        }
    }
}