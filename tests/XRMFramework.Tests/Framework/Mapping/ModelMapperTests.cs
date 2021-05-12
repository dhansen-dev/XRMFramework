using FluentAssertions;

using NUnit.Framework;

using XRMFramework.Mapping;

namespace XRMFramework.Tests.Framework.Mapping
{
    [TestFixture(Category = "Mapping")]
    public class ModelMapperTests
    {
        [Test]
        public void MapTo_AllPropertiesMatch_TargetIsMappedFromSource()
        {
            var modelmapper = new ModelMapper();

            var source = new Source
            {
                Int1 = 1,
                Bool1 = true,
                String1 = "String"
            };

            var target = modelmapper.MapTo<Target>(source);

            target
                .String1
                    .Should().Equals("String");

            target
                .Bool1
                    .Should().BeTrue();

            target
                .Int1
                    .Should().Be(1);
        }

        [Test]
        public void MapTo_TargetHaveBothConstructorAndProperties_TargetIsCreatedWithAllPropertiesPopulated()
        {
            var modelmapper = new ModelMapper();

            var source = new Source
            {
                Int1 = 1,
                Bool1 = true,
                String1 = "String"
            };

            var target = modelmapper.MapTo<TargetWithConstructorAndProperties>(source);

            target
               .String1
                   .Should().Equals("String");

            target
                .Bool1
                    .Should().BeTrue();

            target
                .Int1
                    .Should().Be(1);
        }

        [Test]
        public void MapTo_TargetHaveConstructor_TargetIsCreatedWithConstructorArguments()
        {
            var modelmapper = new ModelMapper();

            var source = new Source
            {
                Int1 = 1,
                Bool1 = true,
                String1 = "String"
            };

            var target = modelmapper.MapTo<TargetWithConstructor>(source);

            target
               .String1
                   .Should().Equals("String");

            target
                .Bool1
                    .Should().BeTrue();

            target
                .Int1
                    .Should().Be(1);
        }

        public class Source : Target
        {
        }

        public class Target
        {
            public bool Bool1 { get; set; }
            public int Int1 { get; set; }
            public string String1 { get; set; }
        }

        public class TargetWithConstructor
        {
            public TargetWithConstructor(string string1, int int1, bool bool1)
            {
                Bool1 = bool1;
                String1 = string1;
                Int1 = int1;
            }

            public bool Bool1 { get; }
            public int Int1 { get; }
            public string String1 { get; }
        }

        public class TargetWithConstructorAndProperties
        {
            public TargetWithConstructorAndProperties(string string1)
            {
                String1 = string1;
            }

            public bool Bool1 { get; set; }
            public int Int1 { get; set; }
            public string String1 { get; }
        }

        public class TargetWithReferenceTypeProperties
        {
            public ReferenceType ReferenceType { get; set; }
        }

        public class TargetWithNestedReferenceTypeProperties
        {
            public ReferenceWithReferenceType ReferenceType { get; }

            public TargetWithNestedReferenceTypeProperties(ReferenceWithReferenceType referenceType)
            {
                ReferenceType = referenceType;
            }
        }

        public class ReferenceWithReferenceType
        {
            public ReferenceType ReferenceType { get; }

            public ReferenceWithReferenceType(ReferenceType referenceType)
            {
                ReferenceType = referenceType;
            }
        }

        public class ReferenceType
        {
            public string Name { get; }

            public ReferenceType(string name)
            {
                Name = name;
            }
        }
    }
}