using System;
using System.Collections.Specialized;
using System.Windows;
using Prism.Regions;

namespace Infrastructure.Prism {
    public class RegionManagerAwareBehavior : RegionBehavior {
        public const string BEHAVIOR_KEY = nameof(RegionManagerAwareBehavior);

        private static void InvokeOnRegionManagerAwareElement(object item, Action<IRegionManagerAware> invocation) {
            if(item is IRegionManagerAware rmAwareItem) {
                invocation(rmAwareItem);
            }

            if(item is FrameworkElement rmAwareFrameWorkElement &&
               rmAwareFrameWorkElement.DataContext is IRegionManagerAware rmAwareDataContext) {

                if(rmAwareFrameWorkElement.Parent is FrameworkElement frameworkElementParent &&
                   frameworkElementParent.DataContext is IRegionManagerAware rmAwareDataContextParent &&
                   rmAwareDataContext == rmAwareDataContextParent) return;

                invocation(rmAwareDataContext);
            }
        }

        protected override void OnAttach() {
            Region.ActiveViews.CollectionChanged += (o,  e) => {
                switch(e.Action) {
                    case NotifyCollectionChangedAction.Add: {
                        foreach(object item in e.NewItems) {
                            IRegionManager regionManager = Region.RegionManager;

                            if(item is FrameworkElement element) {
                                if(element.GetValue(RegionManager.RegionManagerProperty) is IRegionManager
                                       scopedRegionManager) {
                                    regionManager = scopedRegionManager;
                                }
                            }

                            InvokeOnRegionManagerAwareElement(item, x => x.RegionManagerA = regionManager);
                        }
                        break;
                    }
                    case NotifyCollectionChangedAction.Remove: {
                        foreach(object item in e.OldItems)
                            InvokeOnRegionManagerAwareElement(item, x => x.RegionManagerA = null);
                        break;
                    }
                }
            };
        }
    }
}