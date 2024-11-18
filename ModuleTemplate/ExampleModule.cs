using Infrastructure.Constants;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;

namespace ExampleModule {
    /// <summary> Register components of module with Unity/Prism </summary>
    [Module(ModuleName = ModuleNames.EXAMPLE_MODULE)]
    public class ExampleModule : IModule {
        private readonly IRegionManager _regionManager;

        public ExampleModule(IRegionManager regionManager) {
            _regionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterForNavigation<ExampleView>();
        }

        public void OnInitialized(IContainerProvider containerProvider) { }
    }
}
