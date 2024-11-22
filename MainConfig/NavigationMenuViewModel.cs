using GeneralConfig;
using Infrastructure.Prism;
using JetBrains.Annotations;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using TimeOfDayList;
using TimersList;
using static Infrastructure.Constants.RegionNames;

namespace MainConfig {
    [UsedImplicitly]
    public class NavigationMenuViewModel : BindableBase, IRegionManagerAware {
        public DelegateCommand GenConfigButton { get; }
        public DelegateCommand TimersButton { get; }
        public DelegateCommand TimeOfDayButton { get; }

        public enum SelectedButton { GenConfig, Timers, TimeOfDayButton }

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
            TimeOfDayButton = new DelegateCommand(() => {
                RegionManagerA.RequestNavigate(SHELL_CONFIG_REGION, nameof(TimeOfDayListView));
                Selected = SelectedButton.TimeOfDayButton;
            });
        }

        public IRegionManager RegionManagerA { get; set; }
    }
}
