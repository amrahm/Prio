using Infrastructure.Prism;
using Prism.Navigation.Regions;
using Timer;
using static Infrastructure.Constants.RegionNames;

namespace TimeOfDayList {
    /// <summary> Interaction logic for TimersListItemView.xaml </summary>
    public partial class TimeOfDayListItemView : IRegionManagerAware  {
        public TimeOfDayListItemViewModel ViewModel { get; }

        public TimeOfDayListItemView(TimeOfDayListItemViewModel vm) {
            InitializeComponent();
            ViewModel = vm;
            DataContext = ViewModel;
            Loaded += (_,  _) => {
                RegionManagerA.AddToRegionRMAware(TIMER_IN_LIST_REGION, new TimerView(new TimerViewModel(ViewModel.Timer)));
            };
        }

        public IRegionManager RegionManagerA { get; set; }
    }
}
