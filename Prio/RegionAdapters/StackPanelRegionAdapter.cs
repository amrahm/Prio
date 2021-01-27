using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;
using Prism.Regions;

namespace Prio.RegionAdapters {
    [UsedImplicitly]
    public class StackPanelRegionAdapter : RegionAdapterBase<StackPanel> {
        public StackPanelRegionAdapter(IRegionBehaviorFactory factory)
            : base(factory) { }

        protected override void Adapt(IRegion region, StackPanel regionTarget) {
            region.Views.CollectionChanged += (_, e) => {
                switch(e.Action) {
                    case NotifyCollectionChangedAction.Add: {
                        Debug.Assert(e.NewItems != null, "e.NewItems != null");
                        foreach(FrameworkElement element in e.NewItems) {
                            regionTarget.Children.Add(element);
                        }
                        break;
                    }
                    case NotifyCollectionChangedAction.Remove: {
                        Debug.Assert(e.OldItems != null, "e.OldItems != null");
                        foreach(FrameworkElement element in e.OldItems) {
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