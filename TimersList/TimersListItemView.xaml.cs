using Prism.Regions;
using Timer;
using static Infrastructure.Constants.RegionNames;

namespace TimersList {
    /// <summary>
    /// Interaction logic for TimersListItemView.xaml
    /// </summary>
    public partial class TimersListItemView  {
        public TimersListItemView(IRegionManager regionManager) {
            InitializeComponent();
            regionManager.RegisterViewWithRegion(TIMER_IN_LIST_REGION, typeof(TimerView));
        }
    }
}