using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;
using Prism.Regions;

namespace Prio.RegionAdapters {
    [UsedImplicitly]
    public class WrapPanelRegionAdapter : RegionAdapterBase<WrapPanel> {
        public WrapPanelRegionAdapter(IRegionBehaviorFactory factory)
            : base(factory) { }

        protected override void Adapt(IRegion region, WrapPanel regionTarget) {
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