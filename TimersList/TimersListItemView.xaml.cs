using Infrastructure.Prism;
using Prism.Regions;
using Timer;
using static Infrastructure.Constants.RegionNames;

namespace TimersList {
    /// <summary> Interaction logic for TimersListItemView.xaml </summary>
    public partial class TimersListItemView : IRegionManagerAware  {
        public TimersListItemView(TimersListItemViewModel vm) {
            InitializeComponent();
            DataContext = vm;
            Loaded += (o,  e) => RegionManagerA.AddToRegionRMAware(TIMER_IN_LIST_REGION,
                                                                   new TimerView(new TimerViewModel(vm.Timer)));
        }

        public IRegionManager RegionManagerA { get; set; }
    }
}