using XRMFramework.Container;
using XRMFramework.Core;

using FluentAssertions;

using NUnit.Framework;

using System;
using System.Collections.Generic;

namespace XRMFramework.Tests.Container
{
    [TestFixture(Category = nameof(SuperSimpleCRMInjector))]
    public class MapCollectionTests : ContainerTestsBase
    {
        [Test]
        public void MapCollection_TypeIsOpenGeneric_WhenMappingIsCreatedForEachClosedType()
        {
            Injector
                .MapCollection(typeof(IGenericInterface<>))
                .Map<ISomeDTOCollection, SomeDTOCollection>();

            var someDTO = Injector.GetService<ISomeDTOCollection>();

            someDTO.Should().NotBeNull();

            someDTO.Instances.Should()
                .HaveCount(1, "because one concrete type is implemeting IGenericInteface<SomeDTOImpl>")
                .And
                .AllBeOfType<SomeDTOImpl>()
                ;
        }

        [Test]
        public void MapCollection_DecoratorIsRegistreredBeforeCollection_CollectionContainsDecorator()
        {
            Injector
                .Map<HandlerService>()
                .MapDecorator<IEventHandler<MapCollectionCommand>, MapCollectionHandler>(typeof(MapCollectionHandlerDecorator))
                .MapCollection(typeof(IEventHandler<MapCollectionCommand>));


            var publisher = Injector.GetService<HandlerService>();

            var command = new MapCollectionCommand();

            publisher.HandleHandlers(command);

            command.HandledBy.Should().Be(typeof(MapCollectionHandlerDecorator), "because when decorator is mapped before collection the collection should be resolved with the decorator type");
        }

        #region Test classes



        internal interface IGenericInterface<TObject>
        {
            void SomeMethod();
        }

        internal class SomeDTO
        { }

        internal class SomeOtherDTO
        { }

        internal class SomeDTOCollection : ISomeDTOCollection
        {
            public IEnumerable<IGenericInterface<SomeDTO>> Instances { get; }

            public SomeDTOCollection(IEnumerable<IGenericInterface<SomeDTO>> instances)
            {
                Instances = instances;
            }

            public void SomeMethod()
            {
            }
        }

        internal class SomeOtherDTOCollection : ISomeOtherDTOCollection
        {
            private readonly IEnumerable<SomeOtherDTO> _instances;

            public SomeOtherDTOCollection(IEnumerable<SomeOtherDTO> instances)
            {
                _instances = instances;
            }
        }

        internal class SomeDTOImpl : IGenericInterface<SomeDTO>
        {
            public void SomeMethod()
            {
            }
        }

        internal class SomeOtherDTOImpl : IGenericInterface<SomeOtherDTO>
        {
            public void SomeMethod()
            {
            }
        }

        internal interface ISomeOtherDTOCollection
        {
        }

        internal interface ISomeDTOCollection
        {
            IEnumerable<IGenericInterface<SomeDTO>> Instances { get; }
        }

        internal class SomeOtherDTOImplDecorator : IGenericInterface<SomeOtherDTO>
        {
            private readonly IGenericInterface<SomeOtherDTO> _decoratee;

            public SomeOtherDTOImplDecorator(IGenericInterface<SomeOtherDTO> decoratee)
            {
                _decoratee = decoratee;
            }

            public void SomeMethod()
            {
                _decoratee.SomeMethod();
            }
        }

        internal class MapCollectionCommand
        {
            public Type HandledBy { get; set; }
        }

        internal class MapCollectionHandler : IEventHandler<MapCollectionCommand>
        {
            public void Handle(MapCollectionCommand command)
            {
                command.HandledBy = GetType();
            }
        }

        internal class MapCollectionHandlerDecorator : IEventHandler<MapCollectionCommand>
        {
            private readonly IEventHandler<MapCollectionCommand> _decoratee;

            public MapCollectionHandlerDecorator(IEventHandler<MapCollectionCommand> decoratee)
            {
                _decoratee = decoratee;
            }

            public void Handle(MapCollectionCommand command)
            {
                command.HandledBy = GetType();
            }
        }

        internal class HandlerService
        {
            public HandlerService(IEnumerable<IEventHandler<MapCollectionCommand>> handlers)
            {
                _handlers = handlers;
            }

            private readonly IEnumerable<IEventHandler<MapCollectionCommand>> _handlers;

            public void HandleHandlers(MapCollectionCommand command)
            {
                foreach(var handler in _handlers)
                {
                    handler.Handle(command);
                }
            }
        }

        #endregion Test classes
    }
}