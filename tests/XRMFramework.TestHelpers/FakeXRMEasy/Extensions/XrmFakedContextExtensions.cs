using FakeItEasy;

using FakeXrmEasy;

using Microsoft.Xrm.Sdk;

using System;

namespace XRMFramework.TestHelpers.FakeXRMEasy.Extensions
{
    public static class XrmFakedContextExtensions
    {
        public static IServiceProvider GetServiceProvider(this XrmFakedContext source, XrmFakedPluginExecutionContext pluginExecutionContext = null)
            => new FakedServiceProvider(source, pluginExecutionContext);
    }

    public class FakedServiceProvider : IServiceProvider
    {
        private readonly XrmFakedContext _fakedContext;
        private readonly IOrganizationServiceFactory _organizationServiceFactory;
        private readonly IPluginExecutionContext _pluginExecutionContext;

        public FakedServiceProvider(XrmFakedContext fakedContext, XrmFakedPluginExecutionContext pluginExecutionContext = null)
        {
            _fakedContext = fakedContext;

            _organizationServiceFactory = A.Fake<IOrganizationServiceFactory>();
            A.CallTo(() => _organizationServiceFactory.CreateOrganizationService(A<Guid?>.Ignored)).Returns(_fakedContext.GetOrganizationService());

            _pluginExecutionContext = pluginExecutionContext ?? _fakedContext.GetDefaultPluginContext();
        }

        public object GetService(Type serviceType)
        {
            switch (serviceType.Name)
            {
                case nameof(ITracingService):
                    return _fakedContext.GetFakeTracingService();

                case nameof(IOrganizationService):
                    return _fakedContext.GetOrganizationService();

                case nameof(IOrganizationServiceFactory):
                    return _organizationServiceFactory;

                case nameof(IPluginExecutionContext):
                    return _pluginExecutionContext;

                default:
                    throw new ArgumentException("Unknown service requested (" + serviceType.Name + ")");
            }
        }
    }
}