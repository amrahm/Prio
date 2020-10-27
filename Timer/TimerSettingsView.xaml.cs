using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using Infrastructure.SharedResources;
using Prism.Services.Dialogs;
using Color = System.Windows.Media.Color;

namespace Timer {
    /// <summary>
    /// Interaction logic for TimerSettingsView.xaml
    /// </summary>
    public partial class TimerSettingsView {
        private const int MinCtrlWidth = 470;
        private const int ScreenMargin = 50;
        private const int SnappingIncrement = MinCtrlWidth + 50;

        private DialogWindow _window;


        private double _startHeight;

        public TimerSettingsView() {
            InitializeComponent();
            Loaded += (o, e) => {
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

            SizeChanged += (o, e) => {
                const double spacing = 3;

                double newSizeWidth = e.NewSize.Width;
                int maxPerRow = (int) (newSizeWidth / MinCtrlWidth);
                int numPerRow = Math.Min(maxPerRow, MainWrapPanel.Children.Count);
                MainWrapPanel.Width = newSizeWidth + numPerRow * spacing + 20;
                double ctrlWidth = newSizeWidth / numPerRow - (spacing * (numPerRow - 1) + 15) / numPerRow;

                for(int i = 0; i < MainWrapPanel.Children.Count; i++) {
                    FrameworkElement child = (FrameworkElement) MainWrapPanel.Children[i];
                    child.Margin = new Thickness(0, 0, (i + 1) % numPerRow == 0 ? 0 : spacing, spacing);
                    child.Width = ctrlWidth;
                }

                if(_window != null) {
                    _window.SizeToContent = SizeToContent.Height;
                    if(numPerRow == 1) _window.Height = _startHeight;
                }
            };

            MouseDown += (o, e) => {
                if(e.ChangedButton == MouseButton.Left) {
                    DependencyObject scope = FocusManager.GetFocusScope(MainWrapPanel);
                    FocusManager.SetFocusedElement(scope, _window);

                    _window.DragMove();
                }
            };

            TimerSettingsViewModel vm = DataContext as TimerSettingsViewModel;
            KeyDown += (o, e) => {
                if(vm == null) return;


                //if((Keyboard.FocusedElement as FrameworkElement).GetSelfAndAncestors()
                //    .Any(oo => oo.GetType() == typeof(ShortcutSetter))) {
                //    Debug.WriteLine(e.Key);
                //    vm.HandleKeyDown(e);
                //}
            };
        }
    }
}