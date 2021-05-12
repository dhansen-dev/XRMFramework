using FakeXrmEasy;
using FakeXrmEasy.FakeMessageExecutors;

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

using System;
using System.Collections.Generic;

namespace XRMFramework.TestHelpers.FakeXRMEasy.Executors
{
    public class InstantiateTemplateRequestExecutor : IFakeMessageExecutor
    {
        public bool CanExecute(OrganizationRequest request)
        => request is InstantiateTemplateRequest;

        public OrganizationResponse Execute(OrganizationRequest request, XrmFakedContext ctx)
        {
            var instantiateTemplateRequest = request as InstantiateTemplateRequest;

            var email = new Entity("email")
            {
                ["subject"] = instantiateTemplateRequest.TemplateId
            };

            var response = new InstantiateTemplateResponse
            {
                Results =
                {
                    ["EntityCollection"] = new EntityCollection(new List<Entity> { email })
                }
            };

            return response;
        }

        public Type GetResponsibleRequestType()
            => typeof(InstantiateTemplateRequest);
    }
}