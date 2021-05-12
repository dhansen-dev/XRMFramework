using System;
using System.Collections.Generic;
using System.Linq;

namespace XRMFramework.Container
{
    public partial class SuperSimpleCRMInjector
    {
        /// <summary>
        /// Maps a collection of the specified type. If the type is a open generic type,
        /// one IEnumerable<> will be registrered for each closed generic type        ///
        /// </summary>
        /// <param name="type">The type to map to a collection</param>
        /// <returns>this</returns>
        public SuperSimpleCRMInjector MapCollection<TType>()
            => MapCollection(typeof(TType));

        /// <summary>
        /// Maps a collection of the specified type. If the type is a open generic type,
        /// one IEnumerable<> will be registrered for each closed generic type
        /// </summary>
        /// <param name="type">The type to map to a collection</param>
        /// <returns>this</returns>
        public SuperSimpleCRMInjector MapCollection(Type type)
        {
            var typesImplementingAbstraction = GetTypesImplementingAbstraction(type);

            var listtype = typeof(IEnumerable<>);

            if (type.IsGenericType && !type.IsConstructedGenericType)
            {
                var typeDefinition = type.GetGenericTypeDefinition();

                foreach (var genericTypesGroup in typesImplementingAbstraction.GroupBy(x => x.InterfaceType))
                {
                    var genericListOfGenerics = listtype.MakeGenericType(genericTypesGroup.Key);

                    mappings.Add(genericListOfGenerics, new Mapping
                    {
                        InterfaceType = genericListOfGenerics
                    });

                    var mapping = mappings[genericListOfGenerics];

                    foreach (var typeImplementingAbstraction in genericTypesGroup)
                    {
                        var mappingFound = false;
                        foreach (var intf in typeImplementingAbstraction.ImplementedInterfaces)
                        {
                            if (mappings.TryGetValue(intf, out Mapping alreadyRegisterdMapping) && alreadyRegisterdMapping.IsDecorated)
                            {
                                mapping.ChildMapping.Add(alreadyRegisterdMapping);
                                mappings.Remove(alreadyRegisterdMapping.InterfaceType);
                                mappingFound = true;
                                break;
                            }
                        }
                        if (!mappingFound)
                        {
                            mapping.ChildMapping.Add(new Mapping
                            {
                                InterfaceType = typeImplementingAbstraction.InterfaceType,
                                ConcreteType = typeImplementingAbstraction.ConcreteType
                            });
                        }

                        mappingFound = false;
                    }
                }
            }
            else
            {
                var genericList = listtype.MakeGenericType(type);

                mappings.Add(genericList, new Mapping
                {
                    InterfaceType = genericList
                });

                var mapping = mappings[genericList];

                foreach (var typeImplementingAbstraction in typesImplementingAbstraction)
                {
                    var mappingFound = false;
                    foreach (var intf in typeImplementingAbstraction.ImplementedInterfaces)
                    {
                        if (mappings.TryGetValue(intf, out Mapping alreadyRegisterdMapping) && alreadyRegisterdMapping.IsDecorated)
                        {
                            mapping.ChildMapping.Add(alreadyRegisterdMapping);
                            mappings.Remove(alreadyRegisterdMapping.InterfaceType);
                            mappingFound = true;
                            break;
                        }
                    }
                    if (!mappingFound)
                    {
                        mapping.ChildMapping.Add(new Mapping
                        {
                            InterfaceType = typeImplementingAbstraction.InterfaceType,
                            ConcreteType = typeImplementingAbstraction.ConcreteType
                        });
                    }

                    mappingFound = false;
                }
            }

            return this;
        }
    }
}