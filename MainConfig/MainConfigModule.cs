using Infrastructure.Constants;
using Infrastructure.SharedResources;
using Prism.Ioc;
using Prism.Modularity;

namespace MainConfig {
    /// <summary> Register components of module with Unity/Prism </summary>
    [Module(ModuleName = ModuleNames.MAIN_CONFIG)]
    public class MainConfigModule : IModule {
        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterDialog<MainConfigView, MainConfigViewModel>();
            containerRegistry.RegisterSingleton<IMainConfigService, MainConfigService>();
        }

        public void OnInitialized(IContainerProvider containerProvider) {
            if(!Settings.DoSettingsExists())
                containerProvider.Resolve<IMainConfigService>().ShowConfigWindow();
        }
    }
}