using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly IDialogService _dialogService;

        public TimerModule(IUnityContainer container, IDialogService dialogService) {
            _dialogService = dialogService;
            container.RegisterType<ITimer, TimerModel>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterDialog<TimerSettingsView, TimerSettingsViewModel>();
            containerRegistry.RegisterDialog<TimerView, TimerViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider) {
            Dictionary<Guid, TimerConfig> settingsDict = Settings.LoadSettingsDict<TimerConfig>(TIMER);
            foreach(KeyValuePair<Guid, TimerConfig> configPair in settingsDict) {
                _dialogService.Show(nameof(TimerView), new DialogParameters { { nameof(TimerConfig), configPair.Value } }, result => { });
            }
        }
    }
}