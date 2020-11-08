using System;
using System.Collections.Generic;
using Prism.Ioc;
using Prism.Modularity;
using Infrastructure.Constants;
using Infrastructure.SharedResources;
using Prism.Regions;

namespace Timer {
    /// <summary> Register components of module with Unity/Prism </summary>
    [Module(ModuleName = ModuleNames.TIMER)]
    public class TimerModule : IModule {
        public TimerModule(RegionManager regionManager) {
            // Don't need to worry about RegionManagerAware since we don't intend to do navigation within
            regionManager.RegisterViewWithRegion<TimersGeneralConfigView>(RegionNames.GENERAL_CONFIG_TIMERS_REGION);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterSingleton<ITimersService, TimersService>();
            containerRegistry.RegisterDialog<TimerSettingsView, TimerSettingsViewModel>();
            containerRegistry.RegisterDialog<TimerView, TimerViewModel>();
            containerRegistry.Register<ITimer, TimerModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider) {
            Dictionary<Guid, TimerConfig> settingsDict = Settings.LoadSettingsDict<TimerConfig>(ModuleNames.TIMER);
            var timersService = containerProvider.Resolve<ITimersService>();
            if(settingsDict != null) {
                foreach(KeyValuePair<Guid, TimerConfig> configPair in settingsDict) {
                    ITimer timer = new TimerModel(configPair.Value);
                    timersService.Timers.Add(timer);
                    timer.ShowTimer();
                }
            }
        }
    }
}