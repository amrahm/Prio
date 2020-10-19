using Infrastructure.Constants;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using Unity;

namespace TimersList {
    /// <summary>
    ///     Register components of module with Unity/Prism
    /// </summary>
    [Module(ModuleName = ModuleNames.TIMERS_LIST)]
    public class TimersListModule : IModule {
        private readonly IUnityContainer _container;

        public TimersListModule(IUnityContainer container) {
            _container = container;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) { }

        public void OnInitialized(IContainerProvider containerProvider) {
            _container.RegisterTypeForNavigation<TimersListView>();
        }
    }
}