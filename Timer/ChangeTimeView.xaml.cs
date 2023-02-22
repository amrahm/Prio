using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Infrastructure.SharedResources;
using Color = System.Windows.Media.Color;

namespace Timer
{
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

                _window.CenterOnScreen();
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