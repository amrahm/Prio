using Infrastructure.Constants;
using Prism.Ioc;
using Prism.Modularity;

namespace TimerSettings {
    /// <summary>
    ///     Register components of module with Unity/Prism
    /// </summary>
    [Module(ModuleName = ModuleNames.TIMERS_SETTINGS)]
    public class TimerSettingsModule : IModule {
        public TimerSettingsModule() { }

        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterDialog<TimerSettingsView, TimerSettingsViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider) { }
    }
}