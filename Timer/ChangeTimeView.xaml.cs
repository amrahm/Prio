using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Infrastructure.SharedResources;
using Color = System.Windows.Media.Color;

namespace Timer {
    /// <summary> Interaction logic for ChangeTimeWindow.xaml </summary>
    public partial class ChangeTimeView   {
        private Window _window;
        public ChangeTimeView() {
            InitializeComponent();

            ChangeTimeViewModel vm = (ChangeTimeViewModel) DataContext;
            vm.PropertyChanged += (_,  args) => {
                if(args.PropertyName == nameof(vm.Title)) InlineExpression.SetInlineExpression(TitleBlock, vm.Title);
            };
            Loaded += (_, _) => {
                // Window Setup
                _window = Window.GetWindow(this);
                Debug.Assert(_window != null, nameof(_window) + " != null");

                _window.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                PresentationSource mainWindowPresentationSource = PresentationSource.FromVisual(_window);
                Debug.Assert(mainWindowPresentationSource != null, nameof(mainWindowPresentationSource) + " != null");
                Debug.Assert(mainWindowPresentationSource.CompositionTarget != null, "CompositionTarget != null");
                Matrix m = mainWindowPresentationSource.CompositionTarget.TransformToDevice;
                double dpiWidthFactor = m.M11;
                double dpiHeightFactor = m.M22;

                Rectangle screen = _window.CurrentScreen().WorkingArea;
                _window.Left =  (screen.Width - _window.ActualWidth * dpiWidthFactor) / 2 +  screen.Left;
                _window.Top = (screen.Height -  _window.ActualHeight * dpiHeightFactor) / 2 + screen.Top;
                WindowHelpers.MoveWindowInBounds(_window);
            };

            MouseDown += (_, e) => {
                if(e.ChangedButton == MouseButton.Left) {
                    DependencyObject scope = FocusManager.GetFocusScope(Root);
                    FocusManager.SetFocusedElement(scope, _window);
                    _window.DragMove();
                }
            };
        }
    }
}