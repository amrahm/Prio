using System;
using Infrastructure.Prism;
using JetBrains.Annotations;
using Prism.Regions;
using Prism.Ioc;
using static Infrastructure.SharedResources.UnityInstance;

namespace MainConfig {
    [UsedImplicitly]
    public class MainConfigService : IMainConfigService {
        private MainConfigView _configView;
        private readonly IRegionManager _regionManager;

        public MainConfigService(IRegionManager regionManager) {
            _regionManager = regionManager;
        }

        public void ShowConfigWindow() {
            if(_configView != null) {
                _configView.Activate();
                return;
            }

            _configView = Container.Resolve<MainConfigView>();
            _configView.Closed += (_,  _) => {
                _configView = null;
            };

            _regionManager.CreateScopedRMAware(_configView);

            _configView.Show();
        }

        public void CloseConfigWindow() => _configView?.Close();

        public event Action ApplySettingsPress;

        public void InvokeApplySettings() => ApplySettingsPress?.Invoke();
    }
}