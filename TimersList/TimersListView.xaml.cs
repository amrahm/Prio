using System;
using System.Collections.Specialized;
using System.Windows;
using Prism.Regions;
using static Infrastructure.Constants.RegionNames;

namespace TimersList {
    /// <summary>
    /// Interaction logic for TimersListView.xaml
    /// </summary>
    public partial class TimersListView {
        private const int MinCtrlWidth = 250;

        public TimersListView(IRegionManager regionManager) {
            InitializeComponent();

            TimersListViewModel vm = (TimersListViewModel) DataContext;
            vm.Timers.CollectionChanged += (o, e) => {
                IRegion region = regionManager.Regions[TIMERS_LIST_REGION];
                if(e.Action == NotifyCollectionChangedAction.Add)
                    foreach(object newItem in e.NewItems) {
                        region.Add(newItem, null, true);
                        region.Activate(newItem);
                        SizeChangedEventHandler();
                    }
                else if(e.Action == NotifyCollectionChangedAction.Remove)
                    foreach(object newItem in e.NewItems) {
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
    }
}