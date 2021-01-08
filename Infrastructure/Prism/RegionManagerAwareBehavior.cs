using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using Prism.Regions;

namespace Infrastructure.Prism {
    public class RegionManagerAwareBehavior : RegionBehavior {
        public const string BEHAVIOR_KEY = nameof(RegionManagerAwareBehavior);

        private static void InvokeOnRegionManagerAwareElement(object item, Action<IRegionManagerAware> invocation) {
            if(item is IRegionManagerAware rmAwareItem) {
                invocation(rmAwareItem);
            }

            if(item is FrameworkElement {DataContext: IRegionManagerAware rmAwareDataContext} rmAwareFrameWorkElement) {
                if(rmAwareFrameWorkElement.Parent is FrameworkElement
                           {DataContext: IRegionManagerAware rmAwareDataContextParent}  &&
                   rmAwareDataContext == rmAwareDataContextParent) return;

                invocation(rmAwareDataContext);
            }
        }

        protected override void OnAttach() {
            Region.ActiveViews.CollectionChanged += (_,  e) => {
                switch(e.Action) {
                    case NotifyCollectionChangedAction.Add: {
                        Debug.Assert(e.NewItems != null, "e.NewItems != null");
                        foreach(object item in e.NewItems) {
                            IRegionManager regionManager = Region.RegionManager;

                            if(item is FrameworkElement element &&
                               element.GetValue(RegionManager.RegionManagerProperty) is IRegionManager
                                       scopedRegionManager) {
                                regionManager = scopedRegionManager;
                            }

                            InvokeOnRegionManagerAwareElement(item, x => x.RegionManagerA = regionManager);
                        }
                        break;
                    }
                    case NotifyCollectionChangedAction.Remove: {
                        Debug.Assert(e.OldItems != null, "e.OldItems != null");
                        foreach(object item in e.OldItems)
                            InvokeOnRegionManagerAwareElement(item, x => x.RegionManagerA = null);
                        break;
                    }
                }
            };
        }
    }
}