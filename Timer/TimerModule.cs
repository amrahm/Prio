using Infrastructure.Constants;
using Prism.Ioc;
using Prism.Modularity;

namespace Timer {
    /// <summary>
    ///     Register components of module with Unity/Prism
    /// </summary>
    [Module(ModuleName = ModuleNames.TIMER)]
    public class TimerModule : IModule {
        public TimerModule() { }

        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterDialog<TimerSettingsView, TimerSettingsViewModel>();
            containerRegistry.RegisterDialog<TimerView, TimerViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider) { }
    }
}