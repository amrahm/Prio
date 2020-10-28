using System;
using System.Collections.Generic;
using Prism.Ioc;
using Prism.Modularity;
using Unity;
using static Infrastructure.Constants.ModuleNames;
using Infrastructure.SharedResources;
using Prism.Services.Dialogs;

namespace Timer {
    /// <summary>
    ///     Register components of module with Unity/Prism
    /// </summary>
    [Module(ModuleName = TIMER)]
    public class TimerModule : IModule {
        private readonly IUnityContainer _container;
        private readonly IDialogService _dialogService;

        public TimerModule(IUnityContainer container, IDialogService dialogService) {
            _container = container;
            _dialogService = dialogService;
            _container.RegisterType<ITimer, TimerModel>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterSingleton<ITimersService, TimersService>();
            containerRegistry.RegisterDialog<TimerSettingsView, TimerSettingsViewModel>();
            containerRegistry.RegisterDialog<TimerView, TimerViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider) {
            Dictionary<Guid, TimerConfig> settingsDict = Settings.LoadSettingsDict<TimerConfig>(TIMER);
            var timersService = _container.Resolve<ITimersService>();
            foreach(KeyValuePair<Guid, TimerConfig> configPair in settingsDict) {
                ITimer timer = new TimerModel(configPair.Value);
                timersService.Timers.Add(timer);
                timer.ShowTimer();
            }
        }
    }
}