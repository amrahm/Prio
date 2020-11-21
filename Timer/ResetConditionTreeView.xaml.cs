using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Infrastructure.SharedResources;
using Color = System.Drawing.Color;

namespace Timer {
    /// <summary> Interaction logic for ResetConditionTreeView.xaml </summary>
    public partial class ResetConditionTreeView : IDraggable {
        private const int ROTATION = 100;
        public List<UIElement> ChildDraggables { get; } = new List<UIElement>();
        private readonly ResetConditionTreeViewModel _vm;

        public ResetConditionTreeView(ResetConditionTreeViewModel vm) : this(vm, Color.Lavender) { }

        private ResetConditionTreeView(ResetConditionTreeViewModel vm, Color bgColor) {
            _vm = vm;
            DataContext = _vm;
            InitializeComponent();


            _bgColor = bgColor;
            Background = new SolidColorBrush(_bgColor.ToMediaColor());
            Loaded += (sender,  args) => {
                this.InitializeDraggable();

                if(this.TryFindAncestor<IDraggable>() is TimerSettingsView settings) {
                    Background = Brushes.Transparent;
                    settings.ChildDraggables = ChildDraggables;
                }
            };

            _vm.PropertyChanged += (sender,  args) => {
                if(args.PropertyName == nameof(_vm.Tree)) UpdateTreeLayout();
            };
            UpdateTreeLayout();


            PreviewMouseLeftButtonUp += Control_MouseLeftButtonUp;
            MouseMove += Control_MouseMove;
        }

        private void UpdateTreeLayout() {
            if(_vm.Tree.Condition != null) {
                LeftTree.Content = new ResetConditionView(new ResetConditionViewModel(_vm.Tree.Condition));
                Background = Brushes.Transparent;
            } else if(_vm.Tree.Left != null) {
                var item = new ResetConditionTreeView(new ResetConditionTreeViewModel(_vm.Tree.Left),
                                                      _bgColor.Rotate(ROTATION));
                LeftTree.Content = item;
            }

            if(_vm.Tree.Right != null) {
                AndOr.Visibility = Visibility.Visible;
                RightTree.Visibility = Visibility.Visible;
                ResetConditionTreeView item =
                        new ResetConditionTreeView(new ResetConditionTreeViewModel(_vm.Tree.Right),
                                                   _bgColor.Rotate(ROTATION));
                RightTree.Content = item;
            } else {
                AndOr.Visibility = Visibility.Collapsed;
                RightTree.Visibility = Visibility.Collapsed;
            }
        }

        private bool _isDragging;
        private Point _initPosition;
        private readonly Color _bgColor;

        private void Control_MouseLeftButtonUp(object sender, MouseEventArgs mouseEventArgs) {
            if(_isDragging && sender is ResetConditionTreeView draggable) {
                _isDragging = false;
                draggable.RenderTransform = new TranslateTransform();
                draggable.SetGlobalZ(0);
                draggable.ReleaseMouseCapture();
            }
        }

        private void Control_MouseMove(object sender, MouseEventArgs e) {
            if(e.LeftButton != MouseButtonState.Pressed || !(sender is ResetConditionTreeView draggable)) return;

            if(!_isDragging && Mouse.Captured == null && !ChildDraggables.Any(x => x.IsMouseOver) &&
               !(this.TryFindAncestor<IDraggable>() is TimerSettingsView)) {
                _isDragging = true;
                _initPosition = e.GetPosition(Parent as UIElement);
                draggable.SetGlobalZ(999);
                draggable.CaptureMouse();
            }

            if(!_isDragging) return;

            Point currentPosition = e.GetPosition(Parent as UIElement);

            if(!(draggable.RenderTransform is TranslateTransform transform)) {
                transform = new TranslateTransform();
                draggable.RenderTransform = transform;
            }
            transform.X = currentPosition.X - _initPosition.X;
            transform.Y = currentPosition.Y - _initPosition.Y;
        }
    }
}