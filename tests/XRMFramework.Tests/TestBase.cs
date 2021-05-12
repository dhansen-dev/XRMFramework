using FakeItEasy;

using FakeXrmEasy;

using Microsoft.Xrm.Sdk;

using NUnit.Framework;

using System;

using XRMFramework.Core;
using XRMFramework.Plugin;

namespace XRMFramework.Tests
{
    public abstract class TestBase
    {
        protected XrmFakedContext FakedContext { get; private set; }
        protected CustomPluginContext PluginContext { get; private set; }
        protected XRMSettingsContext FakedSettingsContext { get; private set; }
        protected XRMUserContext FakedXrmUserContext { get; private set; }
        protected XrmFakedPluginExecutionContext FakedPluginExecutionContext { get; private set; }
        protected IServiceProvider FakedServiceProvider { get; private set; }

        [SetUp]
        public void Setup()
        {
            FakedContext = new XrmFakedContext();

            FakedPluginExecutionContext = FakedContext.GetDefaultPluginContext();

            PluginContext = new CustomPluginContext(FakedPluginExecutionContext, null, null);

            FakedSettingsContext = A.Fake<XRMSettingsContext>();

            FakedServiceProvider = A.Fake<IServiceProvider>();
            var fakeFactory = A.Fake<IOrganizationServiceFactory>();

            var fakedService = FakedContext.GetOrganizationService();

            fakedService.Create(new Entity("systemuser", PluginContext.UserId)
            {
                Attributes = new AttributeCollection
                {
                    { "fullname", "Test Testsson" }
                }
            });

            A.CallTo(() => FakedServiceProvider.GetService(typeof(IPluginExecutionContext))).Returns(FakedPluginExecutionContext);
            A.CallTo(() => FakedServiceProvider.GetService(typeof(ITracingService))).Returns(A.Fake<ITracingService>());
            A.CallTo(() => FakedServiceProvider.GetService(typeof(IOrganizationServiceFactory))).Returns(fakeFactory);
            A.CallTo(() => fakeFactory.CreateOrganizationService(A<Guid?>.Ignored)).Returns(fakedService);
        }

        protected void ExecutePlugin(string pluginId, IPlugin plugin)
        {
            FakedPluginExecutionContext.OwningExtension = new EntityReference("sdkmessageprocessingstep", Guid.Parse(pluginId));
            plugin.Execute(FakedServiceProvider);
        }
    }
}