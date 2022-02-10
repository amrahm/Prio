using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using Infrastructure.SharedResources;
using Application = System.Windows.Application;
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
                if(_window != null) {
                    InitializeFloatingWindow();
                    SetPositionLock();
                }
                TimerAspectRatioLimits();

                _vm.Timer.Config.PropertyChanged += (_,  args) => {
                    if(args.PropertyName == nameof(TimerConfig.PositionIsLocked)) SetPositionLock();
                };
            };
        }


        private void SetPositionLock() =>
            _window.ResizeMode = _vm.Timer.Config.PositionIsLocked ? ResizeMode.NoResize : ResizeMode.CanResize;


        private void InitializeFloatingWindow() {
            WindowChrome windowChrome = new()  {CaptionHeight = 0, ResizeBorderThickness = new Thickness(6)};
            WindowChrome.SetWindowChrome(_window, windowChrome);
            _window.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            LoadWindowPosition();
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += (_,  _) => LoadWindowPosition();

            MouseLeftButtonDown += DragMoveWindow;

            SizeChanged += (_, _) => SaveWindowPosition();
        }

        private void DragMoveWindow(object o, MouseButtonEventArgs e) {
            if(_window == null || _vm.Timer.Config.PositionIsLocked) return;
            e.Handled = true;
            DependencyObject scope = FocusManager.GetFocusScope(Root);
            FocusManager.SetFocusedElement(scope, _window);

            _window.DragMove();
            _window.MoveWindowInBounds();

            SaveWindowPosition();
        }

        private void SaveWindowPosition() {
            if(_window == null) return;
            WindowPosition newPos = new(_window.Left, _window.Top, _window.ActualWidth, _window.ActualHeight);
            if(!_vm.Timer.Config.WindowPositions.TryGetValue(Screen.AllScreens.Count(), out WindowPosition configPos) ||
               configPos != newPos) {
                _vm.Timer.Config.WindowPositions[Screen.AllScreens.Count()] = newPos;
                _vm.Timer.SaveSettings();
            }
        }

        private void LoadWindowPosition() {
            if(_vm.Timer.Config.WindowPositions.TryGetValue(Screen.AllScreens.Count(), out WindowPosition position)) {
                _window.Left = position.X;
                _window.Top = position.Y;
                _window.Width = position.Width;
                _window.Height = position.Height;
            }

            _window.MoveWindowInBounds();
        }

        private void TimerAspectRatioLimits(object sender = null, SizeChangedEventArgs sizeChangedEventArgs = null) {
            double hToW = TimerText.ActualWidth / TimerText.ActualHeight;
            TimerViewbox.MaxHeight = TimerViewbox.ActualWidth / hToW * 1.5;
            TimerViewbox.MaxWidth = TimerViewbox.ActualHeight * hToW * 1.5;
        }

        private void TimerView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) => _vm.StartStopTimer.Execute();
    }
}