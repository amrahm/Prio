using System;
using System.Collections.Specialized;
using System.Windows;
using Infrastructure.Prism;
using Prism.Regions;
using static Infrastructure.Constants.RegionNames;

namespace TimersList {
    /// <summary> Interaction logic for TimersListView.xaml </summary>
    public partial class TimersListView : IRegionManagerAware {
        private const int MinCtrlWidth = 250;

        public TimersListView() {
            InitializeComponent();

            TimersListViewModel vm = (TimersListViewModel) DataContext;
            IRegion region = null;
            Initialized += (o,  e) => {
                foreach(TimersListItemView timersListItemView in vm.Timers) {
                    region.AddToRegionScopedRMAware(timersListItemView);
                    SizeChangedEventHandler();
                }
            };
            Loaded += (o,  e) => {
                region = RegionManagerA.Regions[TIMERS_LIST_REGION];
                //TODO fix loading timers (breaks when you navigate to/from multiple times
                //foreach(TimersListItemView timersListItemView in vm.Timers) {
                //    region.AddToRegionScopedRMAware(timersListItemView);
                //    SizeChangedEventHandler();
                //}
            };
            vm.Timers.CollectionChanged += (o, e) => {
                if(e.Action == NotifyCollectionChangedAction.Add)
                    foreach(TimersListItemView newItem in e.NewItems) {
                        region.AddToRegionScopedRMAware(newItem);
                        SizeChangedEventHandler();
                    }
                else if(e.Action == NotifyCollectionChangedAction.Remove)
                    foreach(TimersListItemView newItem in e.NewItems) {
                        region.Remove(newItem);
                        SizeChangedEventHandler();
                    }
            };

            void SizeChangedEventHandler(object sender = null, SizeChangedEventArgs sizeChangedEventArgs = null) {
                double newSizeWidth = TimerWrapPanel.ActualWidth;
                double maxPerRow = Math.Floor(newSizeWidth / MinCtrlWidth);
                double ctrlWidth = newSizeWidth / Math.Min(maxPerRow, TimerWrapPanel.Children.Count) - 10;
                foreach(object item in TimerWrapPanel.Children) { ((FrameworkElement) item).Width = ctrlWidth; }
            }

            SizeChanged += SizeChangedEventHandler;
        }

        public IRegionManager RegionManagerA { get; set; }
    }
}