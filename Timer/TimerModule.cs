using System;
using System.Collections.Generic;
using Prism.Ioc;
using Prism.Modularity;
using static Infrastructure.Constants.ModuleNames;
using Infrastructure.SharedResources;

namespace Timer {
    /// <summary>
    ///     Register components of module with Unity/Prism
    /// </summary>
    [Module(ModuleName = TIMER)]
    public class TimerModule : IModule {
        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterSingleton<ITimersService, TimersService>();
            containerRegistry.RegisterDialog<TimerSettingsView, TimerSettingsViewModel>();
            containerRegistry.RegisterDialog<TimerView, TimerViewModel>();
            containerRegistry.Register<ITimer, TimerModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider) {
            Dictionary<Guid, TimerConfig> settingsDict = Settings.LoadSettingsDict<TimerConfig>(TIMER);
            var timersService = containerProvider.Resolve<ITimersService>();
            if(settingsDict != null)
                foreach(KeyValuePair<Guid, TimerConfig> configPair in settingsDict) {
                    ITimer timer = new TimerModel(configPair.Value);
                    timersService.Timers.Add(timer);
                    timer.ShowTimer();
                }
        }
    }
}