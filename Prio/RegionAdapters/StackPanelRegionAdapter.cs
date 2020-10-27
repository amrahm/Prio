using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using Prism.Regions;

namespace Prio.RegionAdapters {
    public class StackPanelRegionAdapter : RegionAdapterBase<StackPanel> {
        public StackPanelRegionAdapter(IRegionBehaviorFactory factory)
            : base(factory) { }

        protected override void Adapt(IRegion region, StackPanel regionTarget) {
            region.Views.CollectionChanged += (s, e) => {
                switch(e.Action) {
                    case NotifyCollectionChangedAction.Add: {
                        foreach(FrameworkElement element in e.NewItems) {
                            regionTarget.Children.Add(element);
                        }
                        break;
                    }
                    case NotifyCollectionChangedAction.Remove: {
                        foreach(FrameworkElement element in e.NewItems) {
                            regionTarget.Children.Remove(element);
                        }
                        break;
                    }
                }
            };
        }

        protected override IRegion CreateRegion() {
            return new AllActiveRegion();
        }
    }
}