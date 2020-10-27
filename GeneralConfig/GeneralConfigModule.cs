using Infrastructure.Constants;
using Prism.Ioc;
using Prism.Modularity;

namespace GeneralConfig {
    /// <summary>
    ///     Register components of module with Unity/Prism
    /// </summary>
    [Module(ModuleName = ModuleNames.GENERAL_CONFIG)]
    public class GeneralConfigModule : IModule {
        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterForNavigation<GeneralConfigView, GeneralConfigViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider) { }
    }
}