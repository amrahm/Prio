using System.Windows;
using Prism.Ioc;
using Prism.Regions;

namespace Infrastructure.Prism {
    public static class RegionManagerAwareExtensions {
        public static IRegionManager SetRMAware(this IRegionManager regionManagerA, object item) {
            if(item is IRegionManagerAware rmAware) rmAware.RegionManagerA = regionManagerA;

            if(item is FrameworkElement rmAwareFrameWorkElement &&
               rmAwareFrameWorkElement.DataContext is IRegionManagerAware rmAwareDataContext) {
                rmAwareDataContext.RegionManagerA = regionManagerA;
            }
            return regionManagerA;
        }

        public static IRegionManager CreateScopedRMAware(this IRegionManager regionManager, DependencyObject item) {
            var scopedRegion = regionManager.CreateRegionManager();
            RegionManager.SetRegionManager(item, scopedRegion);
            return scopedRegion.SetRMAware(item);
        }

        public static void RegisterViewWithRMAware<T>(this IRegionManager regionManagerA, string regionName) {
            IContainerProvider container = SharedResources.UnityInstance.Container;
            T view = container.Resolve<T>();

            regionManagerA.SetRMAware(view).AddToRegion(regionName, view);
        }

        public static void AddToRegionRMAware(this IRegionManager regionManagerA, string regionName, object item) {
            regionManagerA.SetRMAware(item).AddToRegion(regionName, item);
        }

        public static void AddToRegionScopedRMAware(this IRegionManager regionManagerA, string regionName, object item) {
            IRegion region = regionManagerA.Regions[regionName];
            region.Add(item, null, true).SetRMAware(item);
            region.Activate(item);
        }

        public static void AddToRegionScopedRMAware(this IRegion region, object item) {
            region.Add(item, null, true).SetRMAware(item);
            region.Activate(item);
        }
    }
}