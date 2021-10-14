using Microsoft.Xrm.Sdk;

using System;
using System.Collections.Generic;
using System.Linq;

namespace XRMFramework.Plugin
{
    public enum PipelineStage
    {
        Invalid = 0,
        PreValidation = 10,
        PreOperation = 20,
        PostOperation = 40
    };

    /// <summary>
    /// Variables passed to the execute method of the application services.
    /// </summary>
    public class CustomPluginContext
    {
        private readonly IPluginExecutionContext _executionContext;

        /// <summary>
        /// Contains a merge of Target  / Post and PreImage
        /// </summary>
        private Entity MergedEntity { get; set; }

        public CustomPluginContext(IPluginExecutionContext executionContext, string insecureConfiguration, string secureConfiguration)

        {
            _executionContext = executionContext;
            InsecureConfiguration = insecureConfiguration;
            SecureConfiguration = secureConfiguration;
        }

        public CustomPluginContext ParentContext
            => new CustomPluginContext(_executionContext.ParentContext, InsecureConfiguration, SecureConfiguration);

        public int Depth => _executionContext.Depth;

        /// <summary>
        /// The entityreference for the SetState and SetStateDynamicEntity.
        /// </summary>
        public EntityReference EntityMoniker => GetInputParameterObject<EntityReference>("EntityMoniker");

        /// <summary>
        /// Merges the plugin Target with Post and Pre Entity Images
        /// </summary>
        /// <typeparam name="TEntity">The Entity Type</typeparam>
        /// <returns></returns>
        public TEntity EntityMerge<TEntity>() where TEntity : Entity
        {
            if (MergedEntity != null)
            {
                return MergedEntity as TEntity;
            }

            var target = Target;

            Entity mergeEntity;

            if (target != null)
            {
                var copiedEntity = new Entity(target.LogicalName, target.Id)
                {
                    EntityState = target.EntityState,
                    RowVersion = target.RowVersion
                };

                copiedEntity.KeyAttributes.AddRange(target.KeyAttributes.Select(keyAttr => new KeyValuePair<string, object>(keyAttr.Key, keyAttr.Value)));
                copiedEntity.Attributes.AddRange(target.Attributes.Select(attr => new KeyValuePair<string, object>(attr.Key, attr.Value)));

                mergeEntity = copiedEntity;
            }
            else
            {
                mergeEntity = new Entity(TargetEntityReference.LogicalName, TargetEntityReference.Id);
            }

            mergeEntity.Attributes.AddRange(PostImage?.Attributes.Where(post => !mergeEntity.Attributes.Contains(post.Key)));
            mergeEntity.Attributes.AddRange(PreImage?.Attributes.Where(pre => !mergeEntity.Attributes.Contains(pre.Key)));

            MergedEntity = mergeEntity;

            return MergedEntity.ToEntity<TEntity>();
        }

        public bool TryGetTarget<TEntity>(out TEntity target) where TEntity : Entity, new()
        {
            target = null;

            var entity = new TEntity();

            if (entity.LogicalName == Target.LogicalName)
            {
                target = Target.ToEntity<TEntity>();

                return true;
            }

            return false;
        }

        public bool TryGetEntityMerge<TEntity>(out TEntity entityMerge) where TEntity : Entity, new()
        {
            entityMerge = null;

            var entity = new TEntity();

            if (entity.LogicalName == (Target?.LogicalName ?? TargetEntityReference?.LogicalName))
            {
                entityMerge = EntityMerge<TEntity>();

                return true;
            }

            return false;
        }

        public string InsecureConfiguration { get; }
        public bool IsInTransaction => _executionContext.IsInTransaction;
        public string MessageName => _executionContext.MessageName;
        public DateTime OperationCreatedOn => _executionContext.OperationCreatedOn;
        public Guid OperationId => _executionContext.OperationId;
        public EntityReference OwningExtension => _executionContext.OwningExtension;
        public Entity PostImage => _executionContext.PostEntityImages.TryGetValue("postEntityImage", out Entity postImage) ? postImage : null;
        public Entity PreImage => _executionContext.PreEntityImages.TryGetValue("preEntityImage", out Entity preImage) ? preImage : null;
        public EntityReferenceCollection RelatedEntities => GetInputParameterObject<EntityReferenceCollection>("RelatedEntities");
        public Relationship Relationship => GetInputParameterObject<Relationship>("Relationship");
        public string SecureConfiguration { get; set; }
        public PipelineStage Stage => (PipelineStage)_executionContext.Stage;
        public OptionSetValue StateCode => GetInputParameterObject<OptionSetValue>("State");

        /// <summary>
        /// The statuscode for the SetState and SetStateDynamicEntity.
        /// </summary>
        public OptionSetValue StatusCode => GetInputParameterObject<OptionSetValue>("Status");

        public Entity Target
        {
            get
            {
                if (_executionContext.InputParameters.TryGetValue("Target", out object entityObj))
                {
                    return entityObj as Entity;
                }
                else
                {
                    throw new ArgumentException("Couldtn find Target in InputParamters", nameof(Target));
                }
            }
        }

        public object this[string key]
        {
            get => _executionContext.InputParameters[key];
            set => _executionContext.OutputParameters[key] = value;
        }

        public EntityReference TargetEntityReference
            => GetInputParameterObject<EntityReference>("Target");

        public Guid InitiatingUserId => _executionContext.InitiatingUserId;
        public Guid UserId => _executionContext.UserId;

        public Guid CorrelationId => _executionContext.CorrelationId;

        public Guid BusinessUnitId => _executionContext.BusinessUnitId;

        public TReturn GetInputParameter<TReturn>(string key, Func<object, TReturn> mutator = null)
        {
            if (_executionContext == null)
            {
                throw new ArgumentNullException(nameof(_executionContext));
            }

            var inputParameter = _executionContext.InputParameters[key];

            if (mutator == null)
            {
                return (TReturn)inputParameter;
            }
            return mutator(inputParameter);
        }

        public TEntity GetTarget<TEntity>() where TEntity : Entity
                                    => Target.ToEntity<TEntity>();

        public void SetOutputParameter(string parameterName, object value)
            => _executionContext.OutputParameters[parameterName] = value;

        private T GetInputParameterObject<T>(string key)
        {
            var inputObject = GetInputParameterObject(key);

            if (inputObject != null)
            {
                return (T)inputObject;
            }

            return default;
        }

        private object GetInputParameterObject(string key)
            => _executionContext.InputParameters.TryGetValue(key, out object value) ? value : null;
    }
}