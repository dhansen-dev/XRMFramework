using Microsoft.Xrm.Sdk;

using NUnit.Framework;

using System;
using System.Threading.Tasks;

using XrmFramework.Tests.TestData.Models;

using XRMFramework.Core;
using XRMFramework.Plugin;

namespace XRMFramework.Tests.PluginRegistration
{
    [TestFixture(Category = "PluginBase")]
    public class AsyncPluginBaseTests : TestBase
    {
        [Test]
        public void Execute_AsyncDelegateWithDelay_ExecutionTakesTime()
        {
            var id = Guid.NewGuid();

            FakedPluginExecutionContext.InputParameters.Add("Target", new Account
            {
                Id = id
            });

            var asyncPlugin = new AsyncPlugin();

            ExecutePlugin(asyncPlugin.ExtensionId, asyncPlugin);
        }
    }

    public class AsyncPlugin : PluginBase
    {
        public override string ExtensionId => "931ed7d8-7c8b-44ad-9697-d013faa14293";

        public override string ExtensionDescription => "A test plugin using asyncm methods";

        protected override void PluginSteps()
        {
            AddPluginStep<Account>("Async test step", "931ed7d8-7c8b-44ad-9697-d013faa14293", step => step
                .TriggerOnMessage(Message.Create)
                .Execute(async () =>
                {
                    await Task.Delay(3000).ConfigureAwait(false);
                })
            );
        }
    }
}