using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using Infrastructure.SharedResources;
using Prism.Services.Dialogs;
using Timer.Annotations;
using Color = System.Windows.Media.Color;

namespace Timer {
    /// <summary>
    /// Interaction logic for TimerSettingsView.xaml
    /// </summary>
    public partial class TimerSettingsView : INotifyPropertyChanged {
        private const int MinCtrlWidth = 470;
        private const int ScreenMargin = 50;
        private const int SnappingIncrement = MinCtrlWidth + 50;
        private double _ctrlWidth = MinCtrlWidth;

        public double CtrlWidth {
            get => _ctrlWidth;
            set {
                _ctrlWidth = value;
                OnPropertyChanged();
            }
        }

        private DialogWindow _window;


        private double _startHeight;

        public TimerSettingsView() {
            InitializeComponent();
            Loaded += (o, args) => {
                _window = (DialogWindow) Root.Parent;
                WindowChrome windowChrome = new WindowChrome {
                    ResizeBorderThickness = new Thickness(9, 0, 9, 0),
                    CaptionHeight = 0
                };
                WindowChrome.SetWindowChrome(_window, windowChrome);
                _window.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                _startHeight = _window.Height;
                _window.SizeToContent = SizeToContent.Height;

                PresentationSource mainWindowPresentationSource = PresentationSource.FromVisual(_window);
                Debug.Assert(mainWindowPresentationSource != null, nameof(mainWindowPresentationSource) + " != null");
                Debug.Assert(mainWindowPresentationSource.CompositionTarget != null, "CompositionTarget != null");
                Matrix m = mainWindowPresentationSource.CompositionTarget.TransformToDevice;
                double dpiWidthFactor = m.M11;
                double dpiHeightFactor = m.M22;

                Rectangle screen = _window.CurrentScreen().WorkingArea;
                while(screen.Height - ScreenMargin < Root.ActualHeight * dpiHeightFactor &&
                      screen.Width - ScreenMargin > Root.ActualWidth * dpiWidthFactor + SnappingIncrement) {
                    Root.Width += SnappingIncrement;
                    _window.Width += SnappingIncrement;
                    _window.SizeToContent = SizeToContent.WidthAndHeight;
                }

                _window.MaxHeight = (screen.Height - ScreenMargin) / dpiHeightFactor;

                _window.WindowStartupLocation = WindowStartupLocation.Manual;
                _window.Left =  (screen.Width / dpiWidthFactor - _window.ActualWidth) / 2 +  screen.Left;
                _window.Top = (screen.Height / dpiHeightFactor -  _window.ActualHeight) / 2 + screen.Top;
            };

            SizeChanged += (o, args) => {
                const double spacing = 3;

                double newSizeWidth = args.NewSize.Width;
                int maxPerRow = (int) Math.Floor(newSizeWidth / MinCtrlWidth);
                int numPerRow = Math.Min(maxPerRow, MainWrapPanel.Children.Count);
                MainWrapPanel.Width = newSizeWidth + numPerRow * spacing + 20;
                CtrlWidth = newSizeWidth / numPerRow - (spacing * (numPerRow - 1) + 15) / numPerRow;

                for(int i = 0; i < MainWrapPanel.Children.Count; i++) {
                    FrameworkElement child = (FrameworkElement) MainWrapPanel.Children[i];
                    child.Margin = new Thickness(0, 0, (i + 1) % numPerRow == 0 ? 0 : spacing, spacing);
                }

                if(_window != null) {
                    _window.SizeToContent = SizeToContent.Height;
                    if(numPerRow == 1) _window.Height = _startHeight;
                }
            };

            MouseDown += (o, args) => {
                if(args.ChangedButton == MouseButton.Left) {
                    DependencyObject scope = FocusManager.GetFocusScope(MainWrapPanel);
                    FocusManager.SetFocusedElement(scope, _window);

                    _window.DragMove();
                }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}