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

        public class AccountPostCreatePlugin : PluginBase
        {
            public override string ExtensionId => Guid.NewGuid().ToString();

            public override string ExtensionDescription => "";

            protected override void PluginSteps()
            {
                AddPluginStep<Account>("Test_Step", Guid.NewGuid().ToString(), step => step.
                    StepDescription("Test_Step")
                                .FilterOnAttributes(a => a.AccountName)
                    );
            }
        }

        public class AccountPostCreateNewExpressionForFilteringAttributesPlugin : PluginBase
        {
            public override string ExtensionId => Guid.NewGuid().ToString();

            public override string ExtensionDescription => "";

            protected override void PluginSteps()
            {
                AddPluginStep<Account>("Test_Step", Guid.NewGuid().ToString(), step => step.
                    StepDescription("Test_Step")
                                .FilterOnAttributes(a => new { a.AccountName })
                    );
            }
        }
    }
}