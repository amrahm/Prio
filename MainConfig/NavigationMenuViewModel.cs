using GeneralConfig;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using TimersList;
using static Infrastructure.Constants.RegionNames;

namespace MainConfig {
    public class NavigationMenuViewModel : BindableBase {
        public DelegateCommand GenConfigButton { get; }
        public DelegateCommand TimersButton { get; }

        public NavigationMenuViewModel(IRegionManager regionManager) {
            GenConfigButton = new DelegateCommand(() => {
                regionManager.RequestNavigate(SHELL_CONFIG_REGION, nameof(GeneralConfigView));
            });
            TimersButton = new DelegateCommand(() => {
                regionManager.RequestNavigate(SHELL_CONFIG_REGION, nameof(TimersListView));
            });
        }
    }
}