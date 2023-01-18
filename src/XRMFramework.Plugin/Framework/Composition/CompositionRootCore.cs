using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using Microsoft.Xrm.Sdk.Query;

using System;
using System.Linq;

using XRMFramework.Container;
using XRMFramework.Core;
using XRMFramework.DataAccess;
using XRMFramework.Mapping;
using XRMFramework.Net;
using XRMFramework.Plugin;

namespace XRMFramework.Composition
{
    public interface ICompositionRoot : IDisposable
    {
        object ResolveInstance(Type type);
        TType ResolveInstance<TType>();
    }

    public abstract class CompositionRootCore : IDisposable, ICompositionRoot
    {
        private readonly SuperSimpleCRMInjector _container;

        protected CompositionRootCore(
            IServiceProvider serviceProvider, string insecureConfiguration, string secureConfiguration)
        {
            _container = new SuperSimpleCRMInjector();

            AddAssembliesToScan(_container);

            var serviceFactory = serviceProvider.GetService<IOrganizationServiceFactory>();
            var pluginExecutionContext = serviceProvider.GetService<IPluginExecutionContext>();
            var service = serviceFactory.CreateOrganizationService(pluginExecutionContext.InitiatingUserId);
            var adminService = serviceFactory.CreateOrganizationService(null);
            var tracingService = serviceProvider.GetService<ITracingService>();
            var appInsightsLogger = serviceProvider.GetService<ILogger>();

            var customPluginContext = new CustomPluginContext(pluginExecutionContext, insecureConfiguration, secureConfiguration);

            var userContext = new Lazy<XRMUserContext>(() => CreateUserContext(adminService, pluginExecutionContext));
            var settingsContext = new Lazy<XRMSettingsContext>(() => CreateSettingsContext(adminService, pluginExecutionContext));

            _container
              .Map(service)
              .Map(serviceFactory)
              .Map(tracingService)
              .Map(pluginExecutionContext)
              .Map(userContext)
              .Map(settingsContext)
              .Map(customPluginContext)
              .Map<IDateTimeProvider, DateTimeProvider>()
              .Map<ICrmWebClientBuilderFactory, CrmWebClientBuilderFactory>()
              .Map<ICrmWebClientBuilder, CrmWebClientBuilder>()
              .Map<ICRUDOperations>()
              .Map<ModelMapper>()
              .Map(CRMLogger.GetRootLogger(tracingService, appInsightsLogger))
              .Map<ExecutionContext>()
              .Map(typeof(IEventHandler<>))
              .Map(typeof(IEventHandler<,>))
              ;

            AddMappings(_container);

            AfterMappings(_container);
        }

        protected virtual string[] GetSystemUserAttributesForUserContext()
            => Array.Empty<string>();

        public virtual void AddAssembliesToScan(SuperSimpleCRMInjector container)
        {
        }

        public virtual void AddMappings(SuperSimpleCRMInjector container)
        {
        }

        public virtual void AfterMappings(SuperSimpleCRMInjector _container)
        {

        }

        public virtual TType ResolveInstance<TType>()
            => (TType)ResolveInstance(typeof(TType));

        public virtual object ResolveInstance(Type type)
            => _container.GetService(type);

        protected virtual XRMSettingsContext CreateSettingsContext(IOrganizationService adminService, IPluginExecutionContext pluginExecutionContext)
            => new XRMSettingsContext();

        private XRMUserContext CreateUserContext(IOrganizationService adminService, IPluginExecutionContext pluginExecutionContext)
        {
            var defaultAttributes = new[] { "fullname" };

            var userAttributes = GetSystemUserAttributesForUserContext();

            var completeAttributes = defaultAttributes.Union(userAttributes).ToArray();

            var user = adminService.Retrieve("systemuser", pluginExecutionContext.InitiatingUserId, new ColumnSet(completeAttributes));

            return new XRMUserContext(
                user.Id,
                user
                );
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _container?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}