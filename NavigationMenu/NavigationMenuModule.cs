using Infrastructure.Constants;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace NavigationMenu {
    /// <summary>
    ///     Register components of module with Unity/Prism
    /// </summary>
    [Module(ModuleName = ModuleNames.NAVIGATION_MENU)]
    public class NavigationMenuModule : IModule {
        private readonly IRegionManager _regionManager;

        public NavigationMenuModule(IRegionManager regionManager) {
            _regionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) { }

        public void OnInitialized(IContainerProvider containerProvider) {
            _regionManager.RegisterViewWithRegion(RegionNames.MENU_REGION, typeof(NavigationMenuView));
        }
    }
}