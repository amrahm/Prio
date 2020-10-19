using Infrastructure.Constants;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using Unity;

namespace GeneralConfig {
    /// <summary>
    ///     Register components of module with Unity/Prism
    /// </summary>
    [Module(ModuleName = ModuleNames.GENERAL_CONFIG)]
    public class GeneralConfigModule : IModule {
        private readonly IUnityContainer _container;
        private readonly IRegionManager _regionManager;

        public GeneralConfigModule(IUnityContainer container, IRegionManager regionManager) {
            _container = container;
            _regionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) { }

        public void OnInitialized(IContainerProvider containerProvider) {
            _container.RegisterTypeForNavigation<GeneralConfigView>();
            _regionManager.RequestNavigate(RegionNames.SHELL_CONFIG_REGION, nameof(GeneralConfigView));
        }
    }
}