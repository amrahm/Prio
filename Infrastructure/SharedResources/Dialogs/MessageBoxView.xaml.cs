using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using Window = System.Windows.Window;

namespace Infrastructure.SharedResources {
    /// <summary> Interaction logic for ShowMessageView.xaml </summary>
    public partial class MessageBoxView {
        private Window _window;

        public MessageBoxView() {
            InitializeComponent();

            MessageBoxViewModel vm = (MessageBoxViewModel) DataContext;
            vm.PropertyChanged += (_,  args) => {
                if(args.PropertyName == nameof(vm.Message)) InlineExpression.SetInlineExpression(MessageBlock, vm.Message);
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
                _window.Left =  (screen.Width / dpiWidthFactor - _window.ActualWidth) / 2 +  screen.Left;
                _window.Top = (screen.Height / dpiHeightFactor -  _window.ActualHeight) / 2 + screen.Top;
                WindowHelpers.MoveWindowInBounds(_window);
            };

            MouseDown += (_, e) => {
                if(e.ChangedButton == MouseButton.Left) _window.DragMove();
            };
        }
    }
}