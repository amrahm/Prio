using Infrastructure.Constants;
using Infrastructure.SharedResources;
using JetBrains.Annotations;
using Prism.Ioc;
using Prism.Modularity;

namespace MainConfig {
    /// <summary> Register components of module with Unity/Prism </summary>
    [Module(ModuleName = ModuleNames.MAIN_CONFIG)]
    [UsedImplicitly]
    public class MainConfigModule : IModule {
        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterDialog<MainConfigView, MainConfigViewModel>();
            containerRegistry.RegisterSingleton<IMainConfigService, MainConfigService>();
        }

        public void OnInitialized(IContainerProvider containerProvider) {
            if(!Settings.SettingsExists())
                containerProvider.Resolve<IMainConfigService>().ShowConfigWindow();
        }
    }
}