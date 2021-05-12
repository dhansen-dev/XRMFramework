using FakeXrmEasy;
using FakeXrmEasy.FakeMessageExecutors;

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

using System;

namespace XRMFramework.Test.FakeXRMEasy.Executors
{
    public class SendEmailFromTemplateRequestExecutor : IFakeMessageExecutor
    {
        public bool CanExecute(OrganizationRequest request)
            => request is SendEmailFromTemplateRequest;

        public OrganizationResponse Execute(OrganizationRequest request, XrmFakedContext ctx)
        {
            var templateRequest = request as SendEmailFromTemplateRequest;

            var email = templateRequest.Target;

            email.Id = Guid.NewGuid();
            email["subject"] = "Created from template";
            email["description"] = "Template used = " + templateRequest.TemplateId;

            ctx.GetOrganizationService().Create(email);

            var response = new SendEmailFromTemplateResponse
            {
                ResponseName = nameof(SendEmailFromTemplateResponse),
                Results = new ParameterCollection { { "Id", email.Id } }
            };

            return response;
        }

        public Type GetResponsibleRequestType()
            => typeof(SendEmailFromTemplateRequest);
    }
}