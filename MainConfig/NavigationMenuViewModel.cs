using GeneralConfig;
using Infrastructure.Prism;
using JetBrains.Annotations;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using TimersList;
using static Infrastructure.Constants.RegionNames;

namespace MainConfig {
    [UsedImplicitly]
    public class NavigationMenuViewModel : BindableBase, IRegionManagerAware {
        public DelegateCommand GenConfigButton { get; }
        public DelegateCommand TimersButton { get; }

        public enum SelectedButton { GenConfig, Timers }

        public SelectedButton Selected { get; set; } = SelectedButton.GenConfig;

        public NavigationMenuViewModel() {
            GenConfigButton = new DelegateCommand(() => {
                RegionManagerA.RequestNavigate(SHELL_CONFIG_REGION, nameof(GeneralConfigView));
                Selected = SelectedButton.GenConfig;
            });
            TimersButton = new DelegateCommand(() => {
                RegionManagerA.RequestNavigate(SHELL_CONFIG_REGION, nameof(TimersListView));
                Selected = SelectedButton.Timers;
            });
        }

        public IRegionManager RegionManagerA { get; set; }
    }
}