using Microsoft.Xrm.Sdk;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using XRMFramework.Composition;
using XRMFramework.Core;

namespace XRMFramework.Plugin
{
    public enum AsyncAutoDelete
    {
        No = 0,
        Yes = 1
    };

    public enum ImageType
    {
        PreImage = 0,
        PostImage = 1,
        Both = 2
    }

    public enum Mode
    {
        Synchronous = 0,
        Asynchronous = 1
    };

    public enum Stage
    {
        PreValidation = 10,
        PreOperation = 20,
        PostOperation = 40
    };

    public enum SupportedDeployment
    {
        ServerOnly = 0,
        MicrosoftDynamics365ClientForOutlookOnly = 1,
        Both = 2
    }

    public static class Message
    {
        public const string Assign = "Assign";
        public const string Create = "Create";
        public const string Delete = "Delete";
        public const string GrantAccess = "GrantAccess";
        public const string ModifyAccess = "ModifyAccess";
        public const string Retrieve = "Retrieve";
        public const string RetrieveMultiple = "RetrieveMultiple";
        public const string RetrievePrincipalAccess = "RetrievePrincipalAccess";
        public const string RetrieveSharedPrincipalsAndAccess = "RetrieveSharedPrincipalsAndAccess";
        public const string RevokeAccess = "RevokeAccess";
        public const string SetState = "SetState";
        public const string Update = "Update";
    }

    public abstract class PluginBase : ExtensionBase
    {
        protected const int None = -1;

        private readonly HashSet<Type> funcTypes = new HashSet<Type>
        {
            { typeof(Func<>) },
            { typeof(Func<,>) },
            { typeof(Func<,,>) },
            { typeof(Func<,,,>) },
            { typeof(Func<,,,,>) },
            { typeof(Func<,,,,,>) },
            { typeof(Func<,,,,,,>) },
            { typeof(Func<,,,,,,,>) },
            { typeof(Func<,,,,,,,,>) },
            { typeof(Func<,,,,,,,,,>) },
            { typeof(Func<,,,,,,,,,,>) },
            { typeof(Func<,,,,,,,,,,,>) },
            { typeof(Func<,,,,,,,,,,,,>) },
            { typeof(Func<,,,,,,,,,,,,,>) },
            { typeof(Func<,,,,,,,,,,,,,,>) },
            { typeof(Func<,,,,,,,,,,,,,,,>) },
            { typeof(Func<,,,,,,,,,,,,,,,,>) },
        };

        protected PluginBase()
        {
            PluginSteps();
        }

        protected List<PluginStep> RegisteredPluginSteps { get; } = new List<PluginStep>();


        protected override void ExecuteExtension(IServiceProvider serviceProvider)
        {
            using (var root = CreateCompositionRoot(serviceProvider, InsecureConfiguration, SecureConfiguration))
            {
                var pluginContext = serviceProvider.GetService<IPluginExecutionContext>();

                var logger = root.ResolveInstance<CRMLogger>();

                var currentStep = GetCurrentExecutingStep(pluginContext);

                if (currentStep == null)
                {
                    logger.Log($"No plugin step registered for {pluginContext.OwningExtension?.Name} ({pluginContext.OwningExtension?.Id})");

                    return;
                }

                if (MaxDepthHasBeenExceeded(currentStep, pluginContext, logger))
                {
                    return;
                }

                if (PluginMessageDoesNotMatchStepMessage(currentStep, pluginContext, logger))
                {
                    throw new Exception("An invalid plugin registration were discovered");
                }

                try
                {
                    if (currentStep.DelegatePlugin != null)
                    {
                        var dependencies = ResolveDependencies(root, currentStep.DelegatePlugin);

                        var returnObject = currentStep.DelegatePlugin.DynamicInvoke(dependencies);

                        if (returnObject is Task awaitable)
                        {
                            awaitable.ConfigureAwait(false).GetAwaiter().GetResult();
                        }
                    }
                    else
                    {
                        throw new Exception("No plugin delegate were registred for step " + currentStep.Name);
                    }

                }
                catch (TargetInvocationException tie)
                {
                    ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
                }
            }
        }

        private object[] ResolveDependencies(ICompositionRoot root, Delegate dlg)
        {
            var delegateType = dlg.GetType();

            var args = delegateType.GenericTypeArguments;

            if (delegateType.IsGenericType && funcTypes.Contains(delegateType.GetGenericTypeDefinition()))
            {
                args = args.Take(args.Length - 1).ToArray();
            }

            var dependencies = new object[args.Length];

            for (var i = 0; i < args.Length; i++)
            {
                dependencies[i] = root.ResolveInstance(args[i]);
            }

            return dependencies;
        }

        private bool MaxDepthHasBeenExceeded(PluginStep currentStep, IPluginExecutionContext pluginContext, CRMLogger logger)
        {
            if (currentStep.MaxDepth != -1 && pluginContext.Depth > currentStep.MaxDepth)
            {
                logger.Log("Cancelling plugin execution due to plugin depth of " + pluginContext.Depth +
                    " beeing greater than max depth defined for plugin step which is " + currentStep.MaxDepth);

                LogParentExecutions(pluginContext.ParentContext, logger);

                return true;
            }

            return false;
        }

        private bool PluginMessageDoesNotMatchStepMessage(PluginStep currentStep, IPluginExecutionContext context, CRMLogger logger)
        {
            if (currentStep.Message != context.MessageName)
            {
                logger.Log($"The message {context.MessageName} is not valid for step {currentStep.Name} which is registered on {currentStep.Message}");

                LogParentExecutions(context.ParentContext, logger);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Add a plugin step to the calling plugin. The defaults are
        /// <list type="bullet">
        /// <item>MaxDepth = 1</item>
        /// <item>Mode = Synchronous</item>
        /// <item>Stage = Post operation</item>
        /// </list>
        /// </summary>
        /// <typeparam name="TEntity">The Entity to register the plugin for</typeparam>
        /// <param name="stepName">The name of the step. This has to be unique within a plugin</param>
        /// <param name="registration">Fluent registration of a plugin step</param>
        protected void AddPluginStep(string stepName, string stepId, Func<PluginStep<Entity>, PluginStep> registration)
                            => AddPluginStep<Entity>(stepName, stepId, registration);

        /// <summary>
        /// Add a plugin step to the calling plugin. The defaults are
        /// <list type="bullet">
        /// <item>MaxDepth = 1</item>
        /// <item>Mode = Synchronous</item>
        /// <item>Stage = Post operation</item>
        /// </list>
        /// </summary>
        /// <typeparam name="TEntity">The Entity to register the plugin for</typeparam>
        /// <param name="stepName">The name of the step. This has to be unique within a plugin</param>
        /// <param name="registration">Fluent registration of a plugin step</param>
        protected void AddPluginStep<TEntity>(string stepName, string stepId, Func<PluginStep<TEntity>, PluginStep> registration) where TEntity : Entity, new()
        {
            var step = new PluginStep<TEntity>(stepName)
            {
                Id = Guid.Parse(stepId),
                Rank = RegisteredPluginSteps.Count() + 1,
                MaxDepth = 1,
                Mode = Mode.Synchronous,
                Stage = Stage.PostOperation
            };

            registration(step);

            RegisteredPluginSteps.Add(step);
        }



        protected abstract void PluginSteps();

        private PluginStep GetCurrentExecutingStep(IPluginExecutionContext pluginContext)
            => RegisteredPluginSteps.SingleOrDefault(s => s.Id == pluginContext.OwningExtension.Id);
    }

    public class PluginStep<TEntity> : PluginStep where TEntity : Entity, new()
    {
        public PluginStep(string stepName)
        {
            TriggerOnEntity = new TEntity().LogicalName;
            Name = stepName;
        }

        public PluginStep<TEntity> Execute(Action<CustomPluginContext> plugin)
             => Apply(() => DelegatePlugin = plugin);

        public PluginStep<TEntity> Execute<TDependency1>(Action<TDependency1> plugin)
             => Apply(() => DelegatePlugin = plugin);

        public PluginStep<TEntity> Execute<TDependency1>(Action<CustomPluginContext, TDependency1> plugin)
            => Apply(() => DelegatePlugin = plugin);

        public PluginStep<TEntity> Execute<TDependency1, TDependency2>(Action<CustomPluginContext, TDependency1, TDependency2> plugin)
            => Apply(() => DelegatePlugin = plugin);

        public PluginStep<TEntity> Execute<TDependency1, TDependency2, TDependency3>(Action<CustomPluginContext, TDependency1, TDependency2, TDependency3> plugin)
            => Apply(() => DelegatePlugin = plugin);

        public PluginStep<TEntity> Execute<TDependency1, TDependency2, TDependency3, TDependency4>(Action<CustomPluginContext, TDependency1, TDependency2, TDependency3, TDependency4> plugin)
            => Apply(() => DelegatePlugin = plugin);

        public PluginStep<TEntity> Execute(Func<Task> plugin)
          => Apply(() => DelegatePlugin = plugin);

        public PluginStep<TEntity> Execute(Func<CustomPluginContext, Task> plugin)
          => Apply(() => DelegatePlugin = plugin);

        public PluginStep<TEntity> Execute<TDependency1>(Func<TDependency1, Task> plugin)
             => Apply(() => DelegatePlugin = plugin);

        public PluginStep<TEntity> Execute<TDependency1>(Func<CustomPluginContext, TDependency1, Task> plugin)
            => Apply(() => DelegatePlugin = plugin);

        public PluginStep<TEntity> Execute<TDependency1, TDependency2>(Func<CustomPluginContext, TDependency1, TDependency2, Task> plugin)
            => Apply(() => DelegatePlugin = plugin);

        public PluginStep<TEntity> Execute<TDependency1, TDependency2, TDependency3>(Func<CustomPluginContext, TDependency1, TDependency2, TDependency3, Task> plugin)
            => Apply(() => DelegatePlugin = plugin);

        public PluginStep<TEntity> Execute<TDependency1, TDependency2, TDependency3, TDependency4>(Func<CustomPluginContext, TDependency1, TDependency2, TDependency3, TDependency4, Task> plugin)
            => Apply(() => DelegatePlugin = plugin);

        public PluginStep<TEntity> Deployment(SupportedDeployment deployment)
            => Apply(() => SupportedDeployment = deployment);

        public PluginStep<TEntity> ExecuteInStage(Stage stage)
                 => Apply(() => Stage = stage);

        public PluginStep<TEntity> FilterOnAttributes(Expression<Func<TEntity, object>> attributeSelector)
                            => Apply(() => FilteringAttributes = GetAttributeNameFromExpression(attributeSelector).ToArray());

        public PluginStep<TEntity> FilterOnAttributes(Expression<Func<TEntity, object>> attributeSelector, ImageType imageType)
        {
            FilteringAttributes = GetAttributeNameFromExpression(attributeSelector).ToArray();

            return WithImage(imageType, attributeSelector);
        }

        public PluginStep<TEntity> StepDescription(string pluginDescription)
            => Apply(() => Description = pluginDescription);

        public PluginStep<TEntity> PluginName(string pluginName)
            => Apply(() => Name = pluginName);

        public PluginStep<TEntity> RunAsynchronous(AsyncAutoDelete asyncAutoDelete)
                            => Apply(() =>
                            {
                                Mode = Mode.Asynchronous;
                                AsyncAutoDelete = asyncAutoDelete;
                            });

        public PluginStep<TEntity> RunSynchronous()
            => Apply(() => Mode = Mode.Synchronous);

        public PluginStep<TEntity> TriggerOnMessage(string pluginMessage, string route = null)
                            => Apply(() =>
                            {
                                Message = pluginMessage;
                                Route = route;
                            });

        private IEnumerable<string> GetAttributeNameFromExpression(Expression<Func<TEntity, object>> expression)
        {
            var lambda = (LambdaExpression)expression;

            if (lambda.Body.NodeType == ExpressionType.New)
            {
                var newExpression = (NewExpression)lambda.Body;

                foreach (var member in newExpression.Members)
                {
                    yield return GetAttributeName(member.Name);
                }
            }
            else
            {
                yield return GetAttributeName(GetPropertyName(expression));
            }
        }

        public PluginStep<TEntity> WithImage(ImageType imageType, Expression<Func<TEntity, object>> expression)
        {
            if (imageType == ImageType.Both)
            {
                EntityImages.AddRange(new[] { (int)ImageType.PreImage, (int)ImageType.PostImage });
            }
            else
            {
                EntityImages.Add((int)imageType);
            }

            var attributes = GetAttributeNameFromExpression(expression).ToArray();


            if (imageType == ImageType.PreImage)
            {
                PreEntityImageAttributes = attributes;
            }
            else if (imageType == ImageType.PostImage)
            {
                PostEntityImageAttributes = attributes;
            }
            else
            {
                PreEntityImageAttributes = attributes;
                PostEntityImageAttributes = attributes;
            }

            return this;
        }

        public PluginStep<TEntity> WithMaxDepth(int maxDepth)
        {
            if (maxDepth <= 0 && maxDepth != -1)
            {
                throw new ArgumentException("An invalid value for max depth has been entered. Use -1 to signal that there are no max depth set", nameof(maxDepth));
            }

            MaxDepth = maxDepth;

            return this;
        }

        protected PluginStep<TEntity> Apply(Action action)
        {
            action();

            return this;
        }

        protected string GetAttributeName(string propertyName)
        {
            return typeof(TEntity)
                        .GetProperties()
                        .FirstOrDefault(p => p.Name == propertyName)
                        .GetCustomAttribute<AttributeLogicalNameAttribute>()
                        ?.LogicalName ?? throw new NullReferenceException("Couldnt find any attribute name for property " + propertyName);
        }

        protected string GetPropertyName(Expression<Func<TEntity, object>> expression)
        {
            var lambda = (LambdaExpression)expression;
            MemberExpression memberExpression;
            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else
            {
                memberExpression = (MemberExpression)lambda.Body;
            }

            var member = memberExpression.Member;

            return member.Name;
        }
    }

    public class PluginStep
    {
        public static Dictionary<string, string> MessagePropertyNameLookUp { get; } = new Dictionary<string, string>
        {
            { "Assign", "Target" },
            { "Create", "id" },
            { "Delete", "Target" },
            { "DeliverIncoming", "Target" },
            { "DeliverPromote", "Target" },
            { "Route", "Target" },
            { "Send", "emailId" },
            { "SetStateDynamicEntity", "entityMoniker" },
            { "Update", "Target" }
        };

        public Guid Id { get; protected internal set; }
        public AsyncAutoDelete AsyncAutoDelete { get; protected internal set; }
        public string Description { get; protected internal set; }
        public List<int> EntityImages { get; protected internal set; } = new List<int>();
        public string[] FilteringAttributes { get; protected internal set; }
        public int MaxDepth { get; protected internal set; } = int.MaxValue;
        public string Message { get; protected internal set; }
        public Mode Mode { get; protected internal set; }
        public string Name { get; protected internal set; }
        public string[] PostEntityImageAttributes { get; protected internal set; }
        public string[] PreEntityImageAttributes { get; protected internal set; }
        public int Rank { get; protected internal set; }
        public string Route { get; protected internal set; }
        public Stage Stage { get; protected internal set; }
        public SupportedDeployment SupportedDeployment { get; protected internal set; }
        public string TriggerOnEntity { get; protected internal set; }
        public Delegate DelegatePlugin { get; protected internal set; }
    }

    internal static class ServiceProviderExtensions
    {
        public static TService GetService<TService>(this IServiceProvider source)
            => (TService)source.GetService(typeof(TService));
    }
}