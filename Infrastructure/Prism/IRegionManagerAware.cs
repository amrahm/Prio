using Prism.Regions;

namespace Infrastructure.Prism {
    public interface IRegionManagerAware {
        IRegionManager RegionManagerA { get; set; }
    }
}