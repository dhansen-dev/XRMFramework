using XRMFramework.Plugin;

namespace XRMFramework.Core
{
    public class ExecutionContext
    {
        public ExecutionContext(CustomPluginContext pluginContext,
                                XRMSettingsContext settingsContext,
                                XRMUserContext userContext,
                                CRMLogger logger)
        {
            PluginContext = pluginContext;
            SettingsContext = settingsContext;
            UserContext = userContext;
            Logger = logger;
        }

        public CustomPluginContext PluginContext { get; }
        public XRMSettingsContext SettingsContext { get; }
        public XRMUserContext UserContext { get; }
        public CRMLogger Logger { get; }
    }
}
