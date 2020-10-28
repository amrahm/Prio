using System.Windows;
using Prism.Ioc;
using Prism.Regions;

namespace Infrastructure.Prism {
    public static class RegionManagerAware {
        public static void SetRegionManagerAware(this IRegionManager regionManager, object item) {
            if(item is IRegionManagerAware rmAware) rmAware.RegionManagerA = regionManager;

            if(item is FrameworkElement rmAwareFrameWorkElement &&
               rmAwareFrameWorkElement.DataContext is IRegionManagerAware rmAwareDataContext) {
                rmAwareDataContext.RegionManagerA = regionManager;
            }
        }

        
        public static void RegisterViewWithRegionManagerAware<T>(this IRegionManager regionManagerA, string regionName) {
            IContainerProvider container = Infrastructure.SharedResources.UnityInstance.GetContainer();
            T view = container.Resolve<T>();
            
            regionManagerA.SetRegionManagerAware(view);

            regionManagerA.AddToRegion(regionName, view);
        }
    }
}