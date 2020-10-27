using Infrastructure.Constants;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace GeneralConfig {
    /// <summary>
    ///     Register components of module with Unity/Prism
    /// </summary>
    [Module(ModuleName = ModuleNames.GENERAL_CONFIG)]
    public class GeneralConfigModule : IModule {
        private readonly IRegionManager _regionManager;

        public GeneralConfigModule(IRegionManager regionManager) {
            _regionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterForNavigation<GeneralConfigView, GeneralConfigViewModel>();

        }

        public void OnInitialized(IContainerProvider containerProvider) {
            _regionManager.RequestNavigate(RegionNames.SHELL_CONFIG_REGION, nameof(GeneralConfigView));
        }
    }
}