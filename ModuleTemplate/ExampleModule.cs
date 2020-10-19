using Infrastructure.Constants;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using Unity;

namespace ModuleTemplate {
    /// <summary>
    ///     Register components of module with Unity/Prism
    /// </summary>
    [Module(ModuleName = ModuleNames.EXAMPLE_MODULE)]
    public class ExampleModule : IModule {
        private readonly IUnityContainer _container;
        private readonly IRegionManager _regionManager;

        public ExampleModule(IUnityContainer container, IRegionManager regionManager) {
            _container = container;
            _regionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) { }

        public void OnInitialized(IContainerProvider containerProvider) {
            _container.RegisterTypeForNavigation<ExampleView>();
        }
    }
}