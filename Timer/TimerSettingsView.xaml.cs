using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using Infrastructure.SharedResources;
using Prism.Services.Dialogs;
using Timer.Annotations;

namespace Timer {
    /// <summary>
    /// Interaction logic for TimerSettingsView.xaml
    /// </summary>
    public partial class TimerSettingsView : INotifyPropertyChanged {
        private const int MinCtrlWidth = 450;
        private const int SnappingIncrement = MinCtrlWidth + 50;
        private double _ctrlWidth = MinCtrlWidth;

        public double CtrlWidth {
            get => _ctrlWidth;
            set {
                _ctrlWidth = value;
                OnPropertyChanged();
            }
        }

        private DialogWindow _dialogWindow;


        private double _startHeight;

        public TimerSettingsView() {
            InitializeComponent();
            Loaded += (o, args) => {
                _dialogWindow = (DialogWindow) Root.Parent;
                WindowChrome windowChrome = new WindowChrome {
                    ResizeBorderThickness = new Thickness(9, 0, 9, 0),
                    CaptionHeight = 0
                };
                WindowChrome.SetWindowChrome(_dialogWindow, windowChrome);
                _dialogWindow.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                _startHeight = _dialogWindow.Height;
                _dialogWindow.SizeToContent = SizeToContent.Height;

                PresentationSource mainWindowPresentationSource = PresentationSource.FromVisual(_dialogWindow);
                Debug.Assert(mainWindowPresentationSource != null, nameof(mainWindowPresentationSource) + " != null");
                Debug.Assert(mainWindowPresentationSource.CompositionTarget != null, "CompositionTarget != null");
                Matrix m = mainWindowPresentationSource.CompositionTarget.TransformToDevice;
                double dpiWidthFactor = m.M11;
                double dpiHeightFactor = m.M22;

                while(_dialogWindow.CurrentScreen().WorkingArea.Height - 50 < MainWrapPanel.ActualHeight * dpiHeightFactor &&
                      _dialogWindow.CurrentScreen().WorkingArea.Width - 50 >
                      MainWrapPanel.ActualWidth * dpiWidthFactor + SnappingIncrement) {
                    MainWrapPanel.Width += SnappingIncrement;
                    _dialogWindow.Width += SnappingIncrement;
                    _dialogWindow.SizeToContent = SizeToContent.WidthAndHeight;
                }

                _dialogWindow.MaxHeight = (_dialogWindow.CurrentScreen().WorkingArea.Height - 50) / dpiHeightFactor;

                _dialogWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                _dialogWindow.Left =
                    (_dialogWindow.CurrentScreen().WorkingArea.Width / dpiWidthFactor - _dialogWindow.ActualWidth) / 2 +
                    _dialogWindow.CurrentScreen().WorkingArea.Left;
                _dialogWindow.Top = (_dialogWindow.CurrentScreen().WorkingArea.Height / dpiHeightFactor -
                                     _dialogWindow.ActualHeight) / 2 +
                                    _dialogWindow.CurrentScreen().WorkingArea.Top;
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

                if(_dialogWindow != null) {
                    _dialogWindow.SizeToContent = SizeToContent.Height;
                    if(numPerRow == 1) _dialogWindow.Height = _startHeight;
                }
            };

            MouseDown += (o, args) => {
                if(args.ChangedButton == MouseButton.Left) {
                    DependencyObject scope = FocusManager.GetFocusScope(MainWrapPanel);
                    FocusManager.SetFocusedElement(scope, _dialogWindow);

                    _dialogWindow.DragMove();
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