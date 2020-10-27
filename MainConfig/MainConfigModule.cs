using Infrastructure.Constants;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using GeneralConfig;

namespace MainConfig {
    /// <summary>
    ///     Register components of module with Unity/Prism
    /// </summary>
    [Module(ModuleName = ModuleNames.MAIN_CONFIG)]
    public class MainConfigModule : IModule {
        public MainConfigModule() { }

        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterDialog<MainConfigView, MainConfigViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider) {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(RegionNames.MENU_REGION, typeof(NavigationMenuView));
            regionManager.RequestNavigate(RegionNames.SHELL_CONFIG_REGION, nameof(GeneralConfigView));
        }
    }
}