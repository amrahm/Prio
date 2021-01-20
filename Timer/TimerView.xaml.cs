using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using Infrastructure.SharedResources;
using Color = System.Windows.Media.Color;

namespace Timer {
    /// <summary> Interaction logic for TimerView.xaml </summary>
    public partial class TimerView  {
        private TimerWindow _window;
        private readonly TimerViewModel _vm;

        public TimerView(TimerViewModel vm = null) {
            InitializeComponent();
            SizeChanged += TimerAspectRatioLimits;

            if(vm != null) DataContext = vm;
            _vm = (TimerViewModel) DataContext;

            Loaded += (_, _) => {
                _window = Window.GetWindow(this) as TimerWindow; // This ensures the timer is a floating window
                if(_window != null) InitializeFloatingWindow();
                TimerAspectRatioLimits();
            };
        }

        private void InitializeFloatingWindow() {
            WindowChrome windowChrome = new()  {CaptionHeight = 0};
            WindowChrome.SetWindowChrome(_window, windowChrome);
            _window.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            LoadWindowPosition();

            MouseDown += DragMoveWindow;
            SizeChanged += (_, _) => SaveWindowPosition();
        }

        private void DragMoveWindow(object o, MouseButtonEventArgs e) {
            if(_window == null) return;
            if(e.ChangedButton == MouseButton.Left) {
                DependencyObject scope = FocusManager.GetFocusScope(Root);
                FocusManager.SetFocusedElement(scope, _window);

                _window.DragMove();

                SaveWindowPosition();
            }
        }

        private void SaveWindowPosition() {
            if(_window == null) return;
            WindowPosition newPos = new(_window.Left, _window.Top,
                                        _window.ActualWidth, _window.ActualHeight);
            if(!_vm.Timer.Config.WindowPositions.TryGetValue(Screen.AllScreens.Length, out WindowPosition configPos) ||
               configPos != newPos) {
                _vm.Timer.Config.WindowPositions[Screen.AllScreens.Length] = newPos;
                _vm.Timer.SaveSettings();
            }
        }

        private void LoadWindowPosition() {
            if(_vm.Timer.Config.WindowPositions.TryGetValue(Screen.AllScreens.Length, out WindowPosition position)) {
                _window.Left = position.X;
                _window.Top = position.Y;
                _window.Width = position.Width;
                _window.Height = position.Height;
            }

            WindowHelpers.MoveWindowInBounds(_window);
        }

        private void TimerAspectRatioLimits(object sender = null, SizeChangedEventArgs sizeChangedEventArgs = null) {
            double hToW = TimerText.ActualWidth / TimerText.ActualHeight;
            TimerViewbox.MaxHeight = TimerViewbox.ActualWidth / hToW * 1.5;
            TimerViewbox.MaxWidth = TimerViewbox.ActualHeight * hToW * 1.5;
        }
    }
}