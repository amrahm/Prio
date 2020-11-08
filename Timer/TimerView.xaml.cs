using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using Infrastructure.SharedResources;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace Timer {
    /// <summary> Interaction logic for TimerView.xaml </summary>
    public partial class TimerView  {
        private DialogWindow _window;

        public TimerView() {
            InitializeComponent();
            SizeChanged += TimerAspectRatioLimits;

            var vm = (TimerViewModel) DataContext;

            Loaded += (o, e) => {
                _window = Root.Parent as DialogWindow;
                if(_window == null) return;

                WindowChrome windowChrome = new WindowChrome {
                    CaptionHeight = 0
                };
                WindowChrome.SetWindowChrome(_window, windowChrome);
                _window.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                if(vm.Timer.Config.WindowPositions.Count > 0) {
                    Point? configWindowPosition = null;
                    for(int i = Screen.AllScreens.Length; i > 0; i--)
                        if(vm.Timer.Config.WindowPositions.ContainsKey(i))
                            configWindowPosition = vm.Timer.Config.WindowPositions[Screen.AllScreens.Length];
                    Debug.Assert(configWindowPosition.HasValue, nameof(configWindowPosition) + " != null");

                    _window.Left = configWindowPosition.Value.X;
                    _window.Top = configWindowPosition.Value.Y;
                }

                PresentationSource mainWindowPresentationSource = PresentationSource.FromVisual(_window);
                Debug.Assert(mainWindowPresentationSource != null, nameof(mainWindowPresentationSource) + " != null");
                Debug.Assert(mainWindowPresentationSource.CompositionTarget != null, "CompositionTarget != null");
                Matrix m = mainWindowPresentationSource.CompositionTarget.TransformToDevice;
                double dpiWidthFactor = m.M11;
                double dpiHeightFactor = m.M22;

                Rectangle wA = _window.CurrentScreen().WorkingArea;


                // -600
                //-900 + 200 => -100
                _window.Left += Math.Max(wA.Left / dpiWidthFactor - _window.Left, 0);
                _window.Left += Math.Min(wA.Right / dpiWidthFactor - (_window.Left + _window.ActualWidth), 0);
                _window.Top += Math.Max(wA.Top / dpiHeightFactor - _window.Top, 0);
                _window.Top += Math.Min(wA.Bottom / dpiHeightFactor - (_window.Top + _window.ActualHeight), 0);


                IContainerProvider container = UnityInstance.GetContainer();
                var timersService = container.Resolve<ITimersService>();

                foreach(ITimer timer in timersService.Timers) {
                    //TODO move this out of the way if there's an overlap
                }
            };

            MouseDown += (o, e) => {
                if(_window == null) return;
                if(e.ChangedButton == MouseButton.Left) {
                    DependencyObject scope = FocusManager.GetFocusScope(Root);
                    FocusManager.SetFocusedElement(scope, _window);

                    _window.DragMove();

                    Point newPos = new Point(_window.Left, _window.Top);
                    if(!vm.Timer.Config.WindowPositions.TryGetValue(Screen.AllScreens.Length, out Point configPos) ||
                       !configPos.Equals(newPos)) {
                        vm.Timer.Config.WindowPositions[Screen.AllScreens.Length] = newPos;
                        vm.Timer.SaveSettings();
                    }
                }
            };
        }

        private void TimerAspectRatioLimits(object sender, SizeChangedEventArgs sizeChangedEventArgs) {
            double hToW = TimerText.ActualWidth / TimerText.ActualHeight;
            TimerViewbox.MaxHeight = TimerViewbox.ActualWidth / hToW * 1.5;
            TimerViewbox.MaxWidth = TimerViewbox.ActualHeight * hToW * 1.5;
        }
    }
}