using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XRMFramework.Container
{
    public sealed partial class SuperSimpleCRMInjector : IServiceProvider
    {
        private readonly List<Assembly> AssemblyLookUp = new List<Assembly>() { Assembly.GetExecutingAssembly() };
        private readonly Dictionary<Type, object> instances = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Mapping> mappings = new Dictionary<Type, Mapping>();

        public enum Scope
        {
            Singleton = 0,
            Transient = 1,
        }

        public void Dispose()
        {
            foreach (var instance in instances.Reverse())
            {
                if (instance.Value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        public TService GetService<TService>()
            => (TService)GetService(typeof(TService));

        public object GetService(Type service)
        {
            if (!mappings.TryGetValue(service, out Mapping mapping))
            {
                throw new ArgumentException($"Dependency of type {service.Name} has not been mapped", nameof(service));
            }

            if (mapping.Scope == Scope.Singleton && instances.TryGetValue(mapping.InterfaceType, out object cachedInstance))
            {
                if(mapping.IsLazy)
                {
                    var lazyValue = cachedInstance.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public).GetValue(cachedInstance);

                    return lazyValue;
                }

                return cachedInstance;
            }

            var constructor = ValidateConstructorForType(mapping.ConcreteType);

            var graph = new List<object>();

            foreach (var parameter in constructor.GetParameters())
            {
                var parameterType = parameter.ParameterType;
                // If the dependency is a IEnumerable<T>
                if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    // Check if there is a mapping defined for this dependency
                    if (mappings.TryGetValue(parameterType, out Mapping listMapping))
                    {
                        var type = parameter.ParameterType.GenericTypeArguments[0];

                        var listDependency = new List<object>();
                        var childGraph = new List<object>();

                        // For each type defined for the collection,
                        // resolve the dependencies
                        foreach (var childMapping in listMapping.ChildMapping)
                        {
                            var childConstructor = ValidateConstructorForType(childMapping.ConcreteType);

                            foreach (var childParam in childConstructor.GetParameters())
                            {
                                childGraph.Add(GetService(childParam.ParameterType));
                            }

                            var childInstance = ConstructInstance(childMapping, childConstructor, childGraph);

                            listDependency.Add(childInstance);

                            childGraph = new List<object>();
                        }
                        var listType = typeof(List<>);
                        var li = listType.MakeGenericType(type);
                        IList listInstance = (IList)Activator.CreateInstance(li);

                        for (var i = 0; i < listDependency.Count(); i++)
                        {
                            listInstance.Add(listDependency[i]);
                        }

                        graph.Add(listInstance);
                    }
                    else
                    {
                        var type = parameter.ParameterType.GenericTypeArguments[0];

                        var listDependency = new List<object>();
                        var childGraph = new List<object>();
                        foreach (var s in mapping.ChildMapping)
                        {
                            var childConstructor = ValidateConstructorForType(s.ConcreteType);

                            foreach (var childParam in childConstructor.GetParameters())
                            {
                                childGraph.Add(GetService(childParam.ParameterType));
                            }

                            object childInstance = childConstructor.Invoke(childGraph.ToArray());

                            if (s.IsDecorated)
                            {
                                childInstance = ResolveDecorator(s, childInstance);
                            }

                            listDependency.Add(childInstance);

                            childGraph = new List<object>();
                        }
                        var array = Array.CreateInstance(type, listDependency.Count());
                        for (var i = 0; i < listDependency.Count(); i++)
                        {
                            array.SetValue(listDependency[i], i);
                        }

                        graph.Add(array);
                    }
                }
                else
                {
                    graph.Add(GetService(parameter.ParameterType));
                }
            }

            object instance = ConstructInstance(mapping, constructor, graph);

            if (mapping.Scope == Scope.Singleton)
            {
                instances.Add(service, instance);
            }

            return instance;
        }

        private object ResolveDecorator(Mapping mapping, object instance)
        {
            foreach (var childMapping in mapping.ChildMapping)
            {
                var ctor = ValidateConstructorForType(childMapping.ConcreteType);
                var decoratorDependencies = new List<object>();

                foreach (var param in ctor.GetParameters())
                {
                    // If the requested parametertype is the same as the
                    // mapping (the decorated type), we add in to
                    // the object graph
                    if (param.ParameterType == mapping.InterfaceType)
                    {
                        decoratorDependencies.Add(instance);
                    }
                    else // Else we resolve the dependency
                    {
                        decoratorDependencies.Add(GetService(param.ParameterType));
                    }
                }

                // Create the decorated instance
                instance = ctor.Invoke(decoratorDependencies.ToArray());
            }

            return instance;
        }

        private ConstructorInfo ValidateConstructorForType(Type type)
        {
            if (type == null)
            {
                throw new NullReferenceException("Cannot find constrcutor on null object");
            }

            var constructors = type.GetConstructors();

            if (constructors.Count() != 1)
            {
                throw new ArgumentOutOfRangeException("There can only be one constructor defined on a type. The type " + type.Name + " has " + constructors.Count() + " constuctors defined");
            }

            return constructors.Single();
        }

        public SuperSimpleCRMInjector AddAssembliesToScan(params Assembly[] asssemblies)
        {
            AssemblyLookUp.AddRange(asssemblies);

            return this;
        }

        public SuperSimpleCRMInjector Map<TInterface, TConcrete>() where TConcrete : TInterface
            => Map(typeof(TInterface), typeof(TConcrete));

        public SuperSimpleCRMInjector Map<TType>(TType instance) where TType : class
        {
            var instanceType = instance.GetType();
            var abstractedType = typeof(TType);
            var isLazy = false;

            if(instanceType.IsGenericType && instanceType.GetGenericTypeDefinition() == typeof(Lazy<>))
            {
                instanceType = instanceType.GenericTypeArguments[0];
                abstractedType = instanceType;
                isLazy = true;
            }

            mappings.Add(abstractedType, new Mapping { InterfaceType = abstractedType, ConcreteType = instanceType, Scope = Scope.Singleton, IsLazy = isLazy });
            instances.Add(abstractedType, instance);

            return this;
        }

        public SuperSimpleCRMInjector Map<TConcrete>(Scope scope = Scope.Singleton) where TConcrete : class
        {
            mappings.Add(typeof(TConcrete), new Mapping { InterfaceType = typeof(TConcrete), ConcreteType = typeof(TConcrete), Scope = scope });

            return this;
        }

        public SuperSimpleCRMInjector Map(Type abstraction)
        {
            var implementations = GetTypesImplementingAbstraction(abstraction);

            foreach (var implementation in implementations)
            {
                mappings.Add(implementation.InterfaceType, new Mapping
                {
                    ConcreteType = implementation.ConcreteType,
                    InterfaceType = implementation.InterfaceType
                });
            }

            return this;
        }

        public SuperSimpleCRMInjector Map(Type abstraction, Type implementation, Scope scope = Scope.Singleton)
        {
            if (IsComposite(implementation))
            {
                return MapComposite(abstraction, implementation);
            }

            if (!abstraction.IsGenericType)
            {
                mappings.Add(abstraction, new Mapping
                {
                    ConcreteType = implementation,
                    InterfaceType = abstraction,
                    Scope = scope
                });

                return this;
            }

            return MapGenericType(abstraction, implementation, scope);
        }

        public SuperSimpleCRMInjector Map<TInterface, TConcrete>(Scope scope)
            => Map(typeof(TInterface), typeof(TConcrete), scope);

        private object ConstructInstance(Mapping mapping, ConstructorInfo constructor, List<object> graph)
        {
            object instance = constructor.Invoke(graph.ToArray());

            if (instance == null)
            {
                throw new NullReferenceException("Couldnt construct instance of type " + mapping.ConcreteType.Name);
            }

            // If the defined dependency is decorated, resolve all decorator
            // instances and inject the decorated instance into the dependency instead
            // of the decoratee (core type)
            if (mapping.IsDecorated)
            {
                instance = ResolveDecorator(mapping, instance);
            }

            return instance;
        }

        private Type[] GetAssemblyTypes()
            => AssemblyLookUp.SelectMany(t => t.GetTypes()).ToArray();

        private ImplementationType[] GetTypesImplementingAbstraction(Type abstraction, bool filterOutDecorators = true)
        {
            if (abstraction.IsGenericType && !abstraction.IsConstructedGenericType)
            {
                var typeDef = abstraction.GetGenericTypeDefinition();

                var genericTypes = (from type in GetAssemblyTypes()
                                    let implementedInterfacesOnType = type.GetInterfaces()
                                    from intf in implementedInterfacesOnType
                                    where intf.IsGenericType
                                    where intf.GetGenericTypeDefinition() == typeDef
                                    where type.IsClass
                                    select new ImplementationType
                                    {
                                        ConcreteType = type,
                                        InterfaceType = typeDef.MakeGenericType(intf.GenericTypeArguments),
                                        ConstructorParameters = ValidateConstructorForType(type).GetParameters(),
                                        ImplementedInterfaces = type.GetInterfaces()
                                    }
                  );

                if(filterOutDecorators)
                {
                    genericTypes = FilterOutDecorators(genericTypes);
                }

                return genericTypes.ToArray();
            }

            var types = (from type in GetAssemblyTypes()
                         let implementedInterfacesOnType = type.GetInterfaces()
                         from intf in implementedInterfacesOnType
                         where abstraction == intf
                         where type.IsClass
                         select new ImplementationType
                         {
                             ConcreteType = type,
                             InterfaceType = abstraction,
                             ConstructorParameters = ValidateConstructorForType(type).GetParameters(),
                             ImplementedInterfaces = type.GetInterfaces()
                         }
                    );

            if(filterOutDecorators)
            {
                types = FilterOutDecorators(types);
            }


            return types.ToArray();

            IEnumerable<ImplementationType> FilterOutDecorators(IEnumerable<ImplementationType> typesToFilter)
                => typesToFilter.Where(t => t.ConstructorParameters.Any(c => t.ImplementedInterfaces.Any(i => i == c.ParameterType)) == false);
        }

        /// <summary>
        /// If the implementing type implem
        /// </summary>
        /// <param name="implementation"></param>
        /// <returns></returns>
        private bool IsComposite(Type implementation)
        {
            var implementedInterfaces = implementation.GetInterfaces();

            var constructor = ValidateConstructorForType(implementation);

            var genericEnumerableTypeDef = typeof(IEnumerable<>).GetGenericTypeDefinition();
            foreach (var implementedInteface in implementedInterfaces)
            {
                var constructedEnumerableType = genericEnumerableTypeDef.MakeGenericType(implementedInteface);

                var containsEnumerableOfOwnInterface
                    = constructor.GetParameters().Any(constructorParameter => constructorParameter.ParameterType == constructedEnumerableType);

                if (containsEnumerableOfOwnInterface)
                {
                    return true;
                }
            }

            return false;
        }

        private class Mapping
        {
            public List<Mapping> ChildMapping { get; set; } = new List<Mapping>();
            public Type ConcreteType { get; set; }
            public bool IsDecorated { get; set; }
            public Type InterfaceType { get; set; }
            public Scope Scope { get; set; }
            public bool IsLazy { get; set; }
        }
    }

    internal class ImplementationType
    {
        public Type ConcreteType { get; internal set; }
        public Type InterfaceType { get; internal set; }
        public ParameterInfo[] ConstructorParameters { get; internal set; }
        public Type[] ImplementedInterfaces { get; internal set; }
    }
}