using Prism.Navigation.Regions;

namespace Infrastructure.Prism {
    public interface IRegionManagerAware {
        IRegionManager RegionManagerA { get; set; }
    }
}
