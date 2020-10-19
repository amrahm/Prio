using GeneralConfig;
using Infrastructure.Constants;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using TimersList;

namespace NavigationMenu {
    public class NavigationMenuViewModel : BindableBase {
        public DelegateCommand GenConfigButton { get; }
        public DelegateCommand TimersButton { get; }

        public NavigationMenuViewModel(IRegionManager regionManager) {
            GenConfigButton = new DelegateCommand(() => {
                regionManager.RequestNavigate(RegionNames.SHELL_CONFIG_REGION, nameof(GeneralConfigView));
            });
            TimersButton = new DelegateCommand(() => {
                regionManager.RequestNavigate(RegionNames.SHELL_CONFIG_REGION, nameof(TimersListView));
            });
        }
    }
}