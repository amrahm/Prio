using Infrastructure.Constants;
using Prism.Ioc;
using Prism.Modularity;

namespace TimersList {
    /// <summary>
    ///     Register components of module with Unity/Prism
    /// </summary>
    [Module(ModuleName = ModuleNames.TIMERS_LIST)]
    [ModuleDependency(ModuleNames.TIMER)]
    public class TimersListModule : IModule {
        public TimersListModule() { }

        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterForNavigation<TimersListView>();
        }

        public void OnInitialized(IContainerProvider containerProvider) { }
    }
}