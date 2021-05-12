
using Microsoft.Xrm.Sdk;

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace XRMFramework.Plugin
{
    public abstract class CustomAPI : ExtensionBase
    {
        protected override void ExecuteExtension(IServiceProvider serviceProvider)
        {            
            using (var root = CreateCompositionRoot(serviceProvider, InsecureConfiguration, SecureConfiguration))
            {
                var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(x => x.GetGetMethod(false).GetBaseDefinition() == x.GetGetMethod(false));
                var customAPIMethod = GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).Single(x => x.IsSpecialName == false);

                var pluginExecutionContext = root.ResolveInstance<CustomPluginContext>();

                foreach (var property in properties)
                {
                    property.SetValue(this, pluginExecutionContext[property.Name]);
                }

                var parameters = customAPIMethod.GetParameters().ToArray();

                object[] args = null;

                if (parameters.Count() != 0)
                {
                    args = new object[parameters.Count()];
                    for (var i = 0; i < args.Length; i++)
                    {
                        args[i] = root.ResolveInstance(parameters[i].ParameterType);
                    }
                }

                var possibleReturnObject = customAPIMethod.Invoke(this, args);

                if (possibleReturnObject is Task task)
                {
                    task.ConfigureAwait(false).GetAwaiter().GetResult();

                    var resultProperty = task.GetType().GetProperty("Result");
                    var returnValue = resultProperty.GetValue(task);

                    WriteReturnObjectToOutputParameters(returnValue, pluginExecutionContext);
                }
                else if (customAPIMethod.ReturnType == possibleReturnObject?.GetType())
                {
                    WriteReturnObjectToOutputParameters(possibleReturnObject, pluginExecutionContext);
                }
            }

            void WriteReturnObjectToOutputParameters(object returnObject, CustomPluginContext pluginContext)
            {
                var returnProperties = returnObject?.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (var returnProperty in returnProperties)
                {
                    pluginContext[returnProperty.Name] = returnProperty.GetValue(returnObject);
                }
            }
        }
    }
}