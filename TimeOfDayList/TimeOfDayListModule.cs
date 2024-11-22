using JetBrains.Annotations;
using Prism.Ioc;
using Prism.Modularity;
using static Infrastructure.Constants.ModuleNames;

namespace TimeOfDayList {
    /// <summary> Register components of module with Unity/Prism </summary>
    [Module(ModuleName = TIMERS_LIST)]
    [ModuleDependency(TIMER)]
    [UsedImplicitly]
    public class TimeOfDayListModule : IModule {
        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterForNavigation<TimeOfDayListView, TimeOfDayListViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider) { }
    }
}
