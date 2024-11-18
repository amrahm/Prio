using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using Infrastructure.Prism;
using Prism.Navigation.Regions;
using static Infrastructure.Constants.RegionNames;

namespace TimersList {
    /// <summary> Interaction logic for TimersListView.xaml </summary>
    public partial class TimersListView : IRegionManagerAware {
        private const int MIN_CTRL_WIDTH = 250;

        public TimersListView() {
            InitializeComponent();

            TimersListViewModel vm = (TimersListViewModel) DataContext;
            IRegion region = null;
            Loaded += (_,  _) => {
                region = RegionManagerA.Regions[TIMERS_LIST_REGION];
                foreach(TimersListItemView timersListItemView in vm.Timers) {
                    if(!region.ActiveViews.Contains(timersListItemView)) {
                        region.AddToRegionScopedRMAware(timersListItemView);
                        SizeChangedEventHandler();
                    }
                }
            };
            vm.Timers.CollectionChanged += (_, e) => {
                switch(e.Action) {
                    case NotifyCollectionChangedAction.Add: {
                        Debug.Assert(e.NewItems != null, "e.NewItems != null");
                        foreach(TimersListItemView item in e.NewItems) {
                            region.AddToRegionScopedRMAware(item);
                            SizeChangedEventHandler();
                        }
                        break;
                    }
                    case NotifyCollectionChangedAction.Remove: {
                        Debug.Assert(e.OldItems != null, "e.OldItems != null");
                        foreach(TimersListItemView item in e.OldItems) {
                            region.Remove(item);
                            SizeChangedEventHandler();
                        }
                        break;
                    }
                }
            };

            void SizeChangedEventHandler(object sender = null, SizeChangedEventArgs sizeChangedEventArgs = null) {
                double newSizeWidth = TimerWrapPanel.ActualWidth;
                double maxPerRow = Math.Max(1, Math.Floor(newSizeWidth / MIN_CTRL_WIDTH));
                double ctrlWidth = newSizeWidth / Math.Min(maxPerRow, TimerWrapPanel.Children.Count) - 10;
                foreach(object item in TimerWrapPanel.Children) { ((FrameworkElement) item).Width = ctrlWidth; }
            }

            SizeChanged += SizeChangedEventHandler;
        }

        public IRegionManager RegionManagerA { get; set; }
    }
}
