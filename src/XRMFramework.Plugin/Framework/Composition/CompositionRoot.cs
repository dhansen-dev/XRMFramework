using XRMFramework.Plugin;

using Microsoft.Xrm.Sdk;
using System;

namespace XRMFramework.Composition
{
    public partial class CompositionRoot : CompositionRootCore
    {
        public CompositionRoot(
            IServiceProvider serviceProvider,
            string insecureConfiguration,
            string secureConfiguration
            ) : base(serviceProvider, insecureConfiguration, secureConfiguration)
        {
        }
    }
}