using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using GeneralConfig;
using Infrastructure.Constants;
using Infrastructure.Prism;
using Infrastructure.SharedResources;
using Prism.Regions;

namespace MainConfig {
    /// <summary> Interaction logic for MainConfigView.xaml </summary>
    public partial class MainConfigView : IRegionManagerAware {
        private Window _window;
        public MainConfigView() {
            InitializeComponent();
            Loaded += (_,  _) => {
                RegionManagerA.RegisterViewWithRMAware<NavigationMenuView>(RegionNames.MENU_REGION);
                RegionManagerA.RequestNavigate(RegionNames.SHELL_CONFIG_REGION, nameof(GeneralConfigView));

                // Window Setup
                _window = GetWindow(this);
                Debug.Assert(_window != null, nameof(_window) + " != null");
                _window.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                WindowChrome windowChrome = new() {
                    ResizeBorderThickness = new Thickness(9),
                    CaptionHeight = 0
                };
                WindowChrome.SetWindowChrome(_window, windowChrome);

                (double dpiWidthFactor, double dpiHeightFactor) = WindowHelpers.GetDpiFactors(_window);
                _window.CenterOnScreen(dpiWidthFactor, dpiHeightFactor);
            };

            MouseDown += (_, e) => {
                if(e.ChangedButton == MouseButton.Left) {
                    DependencyObject scope = FocusManager.GetFocusScope(Root);
                    FocusManager.SetFocusedElement(scope, _window);
                    _window.DragMove();
                }
            };
        }

        public IRegionManager RegionManagerA { get; set; }
    }
}