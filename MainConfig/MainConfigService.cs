using Infrastructure.Prism;
using Prism.Regions;
using Unity;

namespace MainConfig {
    public class MainConfigService : IMainConfigService {
        private MainConfigView _configView;
        private readonly IUnityContainer _container;
        private readonly IRegionManager _regionManager;

        public MainConfigService(IUnityContainer container, IRegionManager regionManager) {
            _container = container;
            _regionManager = regionManager;
        }

        public void ShowConfigWindow() {
            if(_configView != null) return;

            _configView = _container.Resolve<MainConfigView>();
            _configView.Closed += (o,  e) => {
                _configView = null;
            };
            var scopedRegion = _regionManager.CreateRegionManager();
            RegionManager.SetRegionManager(_configView, scopedRegion);

            scopedRegion.SetRegionManagerAware(_configView);

            _configView.Show();
        }
    }
}