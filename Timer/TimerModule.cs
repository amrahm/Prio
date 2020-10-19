using Infrastructure.Constants;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using Unity;

namespace Timer {
    /// <summary>
    ///     Register components of module with Unity/Prism
    /// </summary>
    [Module(ModuleName = ModuleNames.TIMER)]
    public class TimerModule : IModule {
        private readonly IUnityContainer _container;

        public TimerModule(IUnityContainer container) {
            _container = container;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) { }

        public void OnInitialized(IContainerProvider containerProvider) {
            _container.RegisterTypeForNavigation<TimerView>();
        }
    }
}