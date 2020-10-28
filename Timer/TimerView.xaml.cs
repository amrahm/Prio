using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using Prism.Services.Dialogs;
using Color = System.Windows.Media.Color;

namespace Timer {
    /// <summary>
    /// Interaction logic for TimerView.xaml
    /// </summary>
    public partial class TimerView  {
        private DialogWindow _window;

        public TimerView() {
            InitializeComponent();
            SizeChanged += TimerAspectRatioLimits;


            Loaded += (o, e) => {
                _window = Root.Parent as DialogWindow;
                if(_window == null) return;

                WindowChrome windowChrome = new WindowChrome {
                    CaptionHeight = 0
                };
                WindowChrome.SetWindowChrome(_window, windowChrome);
                _window.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            };

            MouseDown += (o, e) => {
                if(_window == null) return;
                if(e.ChangedButton == MouseButton.Left) {
                    DependencyObject scope = FocusManager.GetFocusScope(Root);
                    FocusManager.SetFocusedElement(scope, _window);

                    _window.DragMove();
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