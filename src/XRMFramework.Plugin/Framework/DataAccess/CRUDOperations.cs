using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

using System;
using System.Linq;
using System.Linq.Expressions;

namespace XRMFramework.DataAccess
{
    public class CRUDOperations
    {
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationServiceFactory _serviceFactory;

        public CRUDOperations(IOrganizationService organizationService, IOrganizationServiceFactory serviceFactory)
        {
            Context = new OrganizationServiceContext(organizationService)
            {
                MergeOption = MergeOption.NoTracking
            };

            _organizationService = organizationService;
            _serviceFactory = serviceFactory;
        }

        private OrganizationServiceContext Context { get; }

        public Guid Create<TEntity>(TEntity entity) where TEntity : Entity
            => _organizationService.Create(entity);

        public void Delete<TEntity>(TEntity entity) where TEntity : Entity
            => _organizationService.Delete(entity.LogicalName, entity.Id);

        public TResponse Execute<TResponse>(OrganizationRequest request) where TResponse : OrganizationResponse
        {
            var response = _organizationService.Execute(request);

            return (TResponse)response;
        }

        public TResult Find<TEntity, TResult>(Guid entityId, Expression<Func<TEntity, TResult>> projection) where TEntity : Entity
                            => Context.CreateQuery<TEntity>().Where(t => t.Id == entityId).Select(projection).Single();

        public IQueryable<TEntity> QueryOver<TEntity>() where TEntity : Entity
            => Context.CreateQuery<TEntity>();

        public TReturn RunAsOtherUser<TReturn>(Guid? userId, Func<CRUDOperations, TReturn> impersonatedAction)
            => impersonatedAction(CreateImpersonatedService(userId));

        public void RunAsOtherUser(Guid? userId, Action<CRUDOperations> impersonatedAction)
            => impersonatedAction(CreateImpersonatedService(userId));

        public TReturn RunAsSystem<TReturn>(Func<CRUDOperations, TReturn> impersonatedAction)
            => RunAsOtherUser(null, impersonatedAction);

        public void RunAsSystem(Action<CRUDOperations> impersonatedAction)
            => RunAsOtherUser(null, impersonatedAction);

        public void Update<TEntity>(TEntity entity) where TEntity : Entity
                    => _organizationService.Update(entity);

        private CRUDOperations CreateImpersonatedService(Guid? userToImpersonate)
        {
            var impersonatedService = _serviceFactory.CreateOrganizationService(userToImpersonate);

            var crud = new CRUDOperations(impersonatedService, _serviceFactory);

            return crud;
        }
    }
}