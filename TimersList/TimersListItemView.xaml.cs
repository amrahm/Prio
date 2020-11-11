using Infrastructure.Prism;
using Prism.Regions;
using Timer;
using static Infrastructure.Constants.RegionNames;

namespace TimersList {
    /// <summary> Interaction logic for TimersListItemView.xaml </summary>
    public partial class TimersListItemView : IRegionManagerAware  {
        public TimersListItemView(ITimer timer) {
            InitializeComponent();
            var vm = (TimersListItemViewModel) DataContext;
            vm.Timer = timer;
            Loaded += (o,  e) =>
                RegionManagerA.AddToRegionRMAware(TIMER_IN_LIST_REGION, new TimerView(new TimerViewModel(timer)));
        }

        public IRegionManager RegionManagerA { get; set; }
    }
}