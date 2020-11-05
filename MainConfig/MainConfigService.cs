using Infrastructure.Prism;
using JetBrains.Annotations;
using Prism.Regions;
using Unity;

namespace MainConfig {
    [UsedImplicitly]
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

            _regionManager.CreateScopedRMAware(_configView);

            _configView.Show();
        }
    }
}