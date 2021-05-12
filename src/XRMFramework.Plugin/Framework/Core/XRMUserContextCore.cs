using Microsoft.Xrm.Sdk;

using System;

namespace XRMFramework.Core
{

    public partial class XRMUserContext
    {
        private readonly Entity _user;

        public XRMUserContext(Guid userId, Entity user)
        {
            UserId = userId;
            FullName = user.GetAttributeValue<string>("fullname");
            _user = user;
        }

        public string FullName { get; }

        public Guid UserId { get; }
    }
}