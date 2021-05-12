using FluentAssertions;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using XRMFramework.Plugin;

namespace XRMFramework.Tests.PluginRegistration
{
    [TestFixture(Category = "PluginBase")]
    public class PluginBaseTests
    {
        [Test]
        public void PluginBase_WhenRegisteringAPlugin_StepShouldBeConfiguredAccordingly()
        {
            var plugin = new AccountPostCreatePlugin();

            var steps = typeof(PluginBase).GetProperty("RegisteredPluginSteps", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(plugin) as List<PluginStep>;

            var step = steps.Single();

            step
                .Description.Should().Be("Test_Step");

            step.Stage.Should().Be(Stage.PostOperation);

            step.FilteringAttributes.Should().Contain("some_attribute").And.HaveCount(1);
        }

        public class AccountPostCreatePlugin : PluginBase
        {
            public override string ExtensionId => Guid.NewGuid().ToString();

            public override string ExtensionDescription => "";

            protected override void PluginSteps()
            {
                AddPluginStep("Test_Step", Guid.NewGuid().ToString(), step => step.
                    StepDescription("Test_Step")
                                .ExecuteInStage(Stage.PostOperation)
                                .RunSynchronous()
                                .FilterOnAttributes("some_attribute")
                    );
            }
        }
    }
}