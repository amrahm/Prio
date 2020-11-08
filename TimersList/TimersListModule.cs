using JetBrains.Annotations;
using Prism.Ioc;
using Prism.Modularity;
using static Infrastructure.Constants.ModuleNames;

namespace TimersList {
    /// <summary> Register components of module with Unity/Prism </summary>
    [Module(ModuleName = TIMERS_LIST)]
    [ModuleDependency(TIMER)]
    [UsedImplicitly]
    public class TimersListModule : IModule {
        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterForNavigation<TimersListView, TimersListViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider) { }
    }
}