using Prism.Ioc;
using Prism.Modularity;
using Infrastructure.Constants;
using JetBrains.Annotations;
using Prism.Navigation.Regions;

namespace Timer {
    /// <summary> Register components of module with Unity/Prism </summary>
    [Module(ModuleName = ModuleNames.TIMER)]
    [UsedImplicitly]
    public class TimerModule : IModule {
        public TimerModule(RegionManager regionManager) {
            // Don't need to worry about RegionManagerAware since we don't intend to do navigation within
            regionManager.RegisterViewWithRegion<TimersGeneralConfigView>(RegionNames.GENERAL_CONFIG_TIMERS_REGION);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterDialog<TimerSettingsView, TimerSettingsViewModel>();
            containerRegistry.RegisterDialog<ChangeTimeView, ChangeTimeViewModel>();
            containerRegistry.Register<ITimer, TimerModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider) => TimersService.Singleton.ShowTimersAtStartup();
    }
}
