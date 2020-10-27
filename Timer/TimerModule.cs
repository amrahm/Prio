using Infrastructure.Constants;
using Prism.Ioc;
using Prism.Modularity;
using Unity;

namespace Timer {
    /// <summary>
    ///     Register components of module with Unity/Prism
    /// </summary>
    [Module(ModuleName = ModuleNames.TIMER)]
    public class TimerModule : IModule {
        public TimerModule(IUnityContainer container) {
            container.RegisterType<ITimer, TimerViewModel>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterDialog<TimerSettingsView, TimerSettingsViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider) { }
    }
}