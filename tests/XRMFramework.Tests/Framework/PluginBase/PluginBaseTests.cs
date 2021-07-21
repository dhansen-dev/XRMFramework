using FluentAssertions;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using XrmFramework.Tests.TestData.Models;

using XRMFramework.Plugin;

namespace XRMFramework.Tests.PluginRegistration
{
    [TestFixture(Category = "PluginBase")]
    public class PluginBaseTests
    {
        [Test]
        public void PluginBase_WhenRegisteringAPlugin_StepShouldBeConfiguredAccordingly()
        {
            var step = GetPluginstepForPlugin<AccountPostCreatePlugin>();

            step
                .Description.Should().Be("Test_Step");

            step.Stage.Should().Be(Stage.PostOperation);

            step.FilteringAttributes.Should().Contain("name").And.HaveCount(1);
        }

        [Test]
        public void PluginBase_WhenRegisteringPluginWithnewExpressionForFilteringAttributes_StepShouldBeConfiguredAccordingly()
        {
            var step = GetPluginstepForPlugin<AccountPostCreateNewExpressionForFilteringAttributesPlugin>();

            step.Description.Should().Be("Test_Step");

            step.Stage.Should().Be(Stage.PostOperation);

            step.FilteringAttributes.Should().Contain("name").And.HaveCount(1);
        }

        public PluginStep GetPluginstepForPlugin<TPlugin>() where TPlugin : new()
        {
            var plugin = Activator.CreateInstance<TPlugin>();

            var steps = typeof(PluginBase).GetProperty("RegisteredPluginSteps", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(plugin) as List<PluginStep>;

            var step = steps.Single();

            return step;
        }

        [Test]
        public void PluginBase_RegisterImagesWithFilteringAttributes_ImageAttributesAndFilteringAttributesAreTheSame()
        {
            var step = GetPluginstepForPlugin<AccountPostCreateWithImageAndFilteringAttributesSetToTheSamePlugin>();

            step.FilteringAttributes.Should().Contain("name").And.HaveCount(1);
            step.EntityImages.Should().HaveCount(2);
            step.PostEntityImageAttributes.Should().Contain("name").And.HaveCount(1);
            step.PreEntityImageAttributes.Should().Contain("name").And.HaveCount(1);
            step.Rank.Should().Be(1);
            step.TriggerOnEntity.Should().Be("account");
            step.Stage.Should().Be(Stage.PostOperation);
            step.Mode.Should().Be(Mode.Synchronous);
            step.Message.Should().Be(Message.Create);
        }

        public class AccountPostCreatePlugin : PluginBase
        {
            public override string ExtensionId => Guid.NewGuid().ToString();

            public override string ExtensionDescription => "";

            protected override void PluginSteps()
            {
                AddPluginStep<Account>(Message.Create, "Test_Step", Guid.NewGuid().ToString(), step => step
                    .StepDescription("Test_Step")
                    .FilterOnAttributes(a => a.AccountName)
                    );
            }
        }

        public class AccountPostCreateWithImageAndFilteringAttributesSetToTheSamePlugin : PluginBase
        {
            public override string ExtensionId => Guid.NewGuid().ToString();

            public override string ExtensionDescription => "";

            protected override void PluginSteps()
            {
                AddPluginStep<Account>(Message.Create, "Test_Step", Guid.NewGuid().ToString(), step => step.
                    StepDescription("Test_Step")
                                .FilterOnAttributes(a => a.AccountName, ImageType.Both)
                    );
            }
        }

        public class AccountPostCreateNewExpressionForFilteringAttributesPlugin : PluginBase
        {
            public override string ExtensionId => Guid.NewGuid().ToString();

            public override string ExtensionDescription => "";

            protected override void PluginSteps()
            {
                AddPluginStep<Account>(Message.Create, "Test_Step", Guid.NewGuid().ToString(), step => step.
                    StepDescription("Test_Step")
                                .FilterOnAttributes(a => new { a.AccountName })
                    );
            }
        }
    }
}