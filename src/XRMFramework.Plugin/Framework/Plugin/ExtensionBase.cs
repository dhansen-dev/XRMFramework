using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;

using System;
using System.Diagnostics;

using XRMFramework.Composition;
using XRMFramework.Core;

namespace XRMFramework.Plugin
{
    public abstract class ExtensionBase : IPlugin
    {

        public abstract string ExtensionId { get; }
        public abstract string ExtensionDescription { get; }

        protected ExtensionBase()
        {

        }

        protected ExtensionBase(string insecureConfiguration) : this()
        {
            InsecureConfiguration = insecureConfiguration;
        }

        protected ExtensionBase(string insecureConfiguration, string secureConfiguration) : this(insecureConfiguration)
        {
            SecureConfiguration = secureConfiguration;
        }

        protected string InsecureConfiguration { get; }
        protected string SecureConfiguration { get; }

        protected abstract void ExecuteExtension(IServiceProvider serviceProvider);

        void IPlugin.Execute(IServiceProvider serviceProvider)
        {
            var timer = Stopwatch.StartNew();

            try
            {
                PrintStartMessage();

                BeforeExecute(serviceProvider);

                ExecuteExtension(serviceProvider);
            }
            catch (InvalidPluginExecutionException ipe)
            {
                HandleError(serviceProvider, ipe);

                throw;
            }
            catch (Exception ex) when(!(ex is InvalidPluginExecutionException))
            {
                HandleError(serviceProvider, ex);

                throw new InvalidPluginExecutionException("Vänligen kontakta en administratör för mer information om vad som gått fel");
            }
            finally
            {
                AfterExecute(serviceProvider);

                PrintEndMessage();
            }

            void PrintStartMessage()
            {
                var tracingService = serviceProvider.Get<ITracingService>();

                var context = serviceProvider.GetService<IPluginExecutionContext>();

                CRMLogger.GetRootLogger(tracingService).Log($"Triggering for {context.PrimaryEntityName} with Id {context.PrimaryEntityId}");
            }

            void PrintEndMessage()
            {
                var tracingService = serviceProvider.Get<ITracingService>();
                CRMLogger.GetRootLogger(tracingService).Log("Total execution time: " + timer.ElapsedMilliseconds + "ms");
            }

        }

        protected virtual void AfterExecute(IServiceProvider serviceProvider)
        {
        }

        protected virtual void BeforeExecute(IServiceProvider serviceProvider)
        {
        }

        protected virtual ICompositionRoot CreateCompositionRoot(IServiceProvider serviceProvider, string insecureConfiguration, string secureConfiguration)
                         => new CompositionRoot(serviceProvider, insecureConfiguration, secureConfiguration);

        protected virtual void OnError(IServiceProvider serviceProvider, Exception exception)
        {
        }

        private void HandleError(IServiceProvider serviceProvider, Exception ex)
        {
            var pluginExecutionContext = serviceProvider.GetService<IPluginExecutionContext>();
            var tracingService = serviceProvider.GetService<ITracingService>();

            var logger = CRMLogger.GetRootLogger(tracingService);

            OnError(serviceProvider, ex);

            logger
                .LogExecutionContext(pluginExecutionContext)
                .LogException(ex, true, true)
                .LogTime();

        }

        protected static IPluginExecutionContext LogParentExecutions(IPluginExecutionContext parentContext, CRMLogger logger)
        {
            var nestedLogger = logger;
            while (parentContext != null)
            {
                nestedLogger =
                    nestedLogger
                        .NewIndentedBlock()
                            .Log($"Owning {parentContext.OwningExtension?.Id} {parentContext.OwningExtension?.Name}")
                            .LogExecutionContext(parentContext);

                parentContext = parentContext.ParentContext;
            }

            return parentContext;
        }
    }
}
