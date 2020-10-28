using Infrastructure.Prism;
using Prism.Regions;
using Timer;
using static Infrastructure.Constants.RegionNames;

namespace TimersList {
    /// <summary>
    /// Interaction logic for TimersListItemView.xaml
    /// </summary>
    public partial class TimersListItemView : IRegionManagerAware  {
        public ITimer Timer { get; }

        public TimersListItemView(ITimer timer) {
            Timer = timer;
            InitializeComponent();
            Loaded += (o,  e) => {
                RegionManagerA.AddToRegionRMAware(TIMER_IN_LIST_REGION, new TimerView {
                    DataContext = new TimerViewModel(Timer)
                });
            };
        }

        public IRegionManager RegionManagerA { get; set; }
    }
}