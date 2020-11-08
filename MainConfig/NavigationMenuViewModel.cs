using GeneralConfig;
using Infrastructure.Prism;
using JetBrains.Annotations;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using TimersList;
using static Infrastructure.Constants.RegionNames;

namespace MainConfig {
    public class NavigationMenuViewModel : BindableBase, IRegionManagerAware {
        public DelegateCommand GenConfigButton { [UsedImplicitly] get; }
        public DelegateCommand TimersButton { [UsedImplicitly] get; }

        public NavigationMenuViewModel() {
            GenConfigButton =
                new DelegateCommand(() => RegionManagerA.RequestNavigate(SHELL_CONFIG_REGION, nameof(GeneralConfigView)));
            TimersButton =
                new DelegateCommand(() => RegionManagerA.RequestNavigate(SHELL_CONFIG_REGION, nameof(TimersListView)));
        }

        public IRegionManager RegionManagerA { get; set; }
    }
}