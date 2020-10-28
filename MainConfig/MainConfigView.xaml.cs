using GeneralConfig;
using Infrastructure.Constants;
using Infrastructure.Prism;
using Prism.Regions;

namespace MainConfig {
    /// <summary>
    ///     Interaction logic for ShellView.xaml
    /// </summary>
    public partial class MainConfigView : IRegionManagerAware {
        public MainConfigView() {
            InitializeComponent();
            Loaded += (o,  e) => {
                RegionManagerA.RegisterViewWithRMAware<NavigationMenuView>(RegionNames.MENU_REGION);
                RegionManagerA.RequestNavigate(RegionNames.SHELL_CONFIG_REGION, nameof(GeneralConfigView));
            };
        }

        public IRegionManager RegionManagerA { get; set; }
    }
}