using System.Windows;
using System.Windows.Controls;
using Infrastructure.Prism;
using Prio.RegionAdapters;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Prio {
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        protected override void RegisterTypes(IContainerRegistry containerRegistry) { }

        protected override Window CreateShell() {
            Current.Resources.Add(Infrastructure.SharedResources.UnityInstance.CONTAINER_NAME, Container);
            FindResource("MyNotifyIcon");
            return null;
        }

        protected override IModuleCatalog CreateModuleCatalog() {
            return new DirectoryModuleCatalog {ModulePath = @"."};
        }

        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings) {
            base.ConfigureRegionAdapterMappings(regionAdapterMappings);
            regionAdapterMappings.RegisterMapping<StackPanel>(Container.Resolve<StackPanelRegionAdapter>());
            regionAdapterMappings.RegisterMapping<WrapPanel>(Container.Resolve<WrapPanelRegionAdapter>());
        }

        protected override void ConfigureDefaultRegionBehaviors(IRegionBehaviorFactory regionBehaviors) {
            base.ConfigureDefaultRegionBehaviors(regionBehaviors);
            regionBehaviors.AddIfMissing(RegionManagerAwareBehavior.BEHAVIOR_KEY, typeof(RegionManagerAwareBehavior));
        }
    }
}