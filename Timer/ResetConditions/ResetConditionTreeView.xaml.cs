using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Infrastructure.SharedResources;
using Color = System.Drawing.Color;

namespace Timer {
    /// <summary> Interaction logic for ResetConditionTreeView.xaml </summary>
    public partial class ResetConditionTreeView : IDraggable {
        private const int ROTATION = 100;
        public List<UIElement> ChildDraggables { get; } = new();
        private readonly ResetConditionTreeViewModel _vm;
        private readonly Storyboard _opacityStoryboard = new();
        private static readonly Duration Duration = new(TimeSpan.FromMilliseconds(200));
        private  DispatcherTimer _unhighlightTimer;
        private  bool _isRoot;

        public ResetConditionTreeView(ResetConditionTreeViewModel vm) : this(vm, Color.Lavender) { }

        private ResetConditionTreeView(ResetConditionTreeViewModel vm, Color bgColor) {
            _vm = vm;
            _bgColor = bgColor;
            DataContext = _vm;
            InitializeComponent();

            Loaded += (_,  _) => {
                this.InitializeDraggable();

                if(this.TryFindAncestor<IDraggable>() is TimerSettingsView settings) {
                    _isRoot = true;
                    Background = Brushes.Transparent;
                    settings.ChildDraggables = ChildDraggables;

                    _vm.PropertyChanged += (_,  e) => {
                        if(e.PropertyName == nameof(_vm.Tree)) {
                            ChildDraggables.Clear();
                            UpdateTreeLayout();
                        }
                    };
                    UpdateTreeLayout(); //needed for adding first child
                }
            };
            UpdateTreeLayout(); //needed to set height properly before settings view is fully loaded

            PreviewMouseLeftButtonUp += Control_MouseLeftButtonUpPreview;
            MouseLeftButtonUp += Control_MouseLeftButtonUp;
            MouseMove += Control_MouseMove;
        }

        private void UpdateTreeLayout() {
            if(_vm.Tree.IsBranch) {
                Condition.Content = null;
                LeftTree.Content = _leftTreeContent = new ResetConditionTreeView(
                    new ResetConditionTreeViewModel(_vm.Tree.Left), _bgColor.Rotate(ROTATION)
                );
                RightTree.Content = _rightTreeContent = new ResetConditionTreeView(
                    new ResetConditionTreeViewModel(_vm.Tree.Right), _bgColor.Rotate(ROTATION)
                );

                if(!_isRoot) Background = new SolidColorBrush(_bgColor.ToMediaColor());
                AndOr.Visibility = Visibility.Visible;
            } else {
                Condition.Content = _vm.Tree.Condition != null ?
                        new ResetConditionView(new ResetConditionViewModel(_vm.Tree.Condition)) :
                        null;
                LeftTree.Content = _leftTreeContent = null;
                RightTree.Content = _rightTreeContent = null;

                Background = Brushes.Transparent;
                AndOr.Visibility = Visibility.Collapsed;
            }
        }

        private bool _isDragging;
        private Point _initPosition;
        private readonly Color _bgColor;
        private ResetConditionTreeView _leftTreeContent;
        private ResetConditionTreeView _rightTreeContent;

        private void Control_MouseLeftButtonUpPreview(object sender, MouseEventArgs e) {
            if(_isDragging || !_vm.Tree.IsBranch) return;
            if(Mouse.Captured is ResetConditionTreeView dropped &&
               !AnyChildIsDropCandidate(dropped, c => c.Control_MouseLeftButtonUpPreview(sender, e))) {
                bool toLeft = e.GetPosition(AndOr).Y < AndOr.ActualHeight / 2;
                FrameworkElement addingTo = toLeft ? _leftTreeContent : _rightTreeContent;
                bool toLeftOfAdded = e.GetPosition(addingTo).Y > addingTo.ActualHeight / 2;
                dropped.Control_MouseLeftButtonUp(dropped, e);
                _vm.Tree.MoveNode(dropped._vm.Tree, toLeft, toLeftOfAdded);
            }
        }

        /// <summary> Checks if any child is under the mouse, since they should get priority.
        ///           If they are, call callback with them as a parameter </summary>
        private bool AnyChildIsDropCandidate(ResetConditionTreeView dropped,
                                             Action<ResetConditionTreeView> callback = null) => ChildDraggables.Any(
            delegate(UIElement x) {
                if(x != dropped && x is ResetConditionTreeView c && c.ContainsMouse() && c._vm.Tree.IsBranch) {
                    callback?.Invoke(c);
                    return true;
                }
                return false;
            }
        );

        private void Control_MouseLeftButtonUp(object sender, MouseEventArgs e) {
            if(_isDragging && sender is ResetConditionTreeView draggable) {
                _isDragging = false;
                draggable.RenderTransform = new TranslateTransform();
                draggable.ReleaseMouseCapture();
                draggable.SetGlobalZ(0);
                AnimateOpacity(1);
            }
        }

        private void Control_MouseMove(object sender, MouseEventArgs e) {
            if(e.LeftButton != MouseButtonState.Pressed || sender is not ResetConditionTreeView draggable) return;

            if(!_isDragging && Mouse.Captured == null && !ChildDraggables.Any(x => x.IsMouseOver) && !_isRoot) {
                _isDragging = true;
                _initPosition = e.GetPosition(Parent as UIElement);
                draggable.SetGlobalZ(999);
                draggable.CaptureMouse();
                AnimateOpacity(.6);
            }

            if(_isDragging) {
                Point currentPosition = e.GetPosition(Parent as UIElement);
                if(draggable.RenderTransform is not TranslateTransform transform) {
                    transform = new TranslateTransform();
                    draggable.RenderTransform = transform;
                }
                transform.X = currentPosition.X - _initPosition.X;
                transform.Y = currentPosition.Y - _initPosition.Y;
            } else if(_vm.Tree.IsBranch) {
                if(Mouse.Captured is ResetConditionTreeView toDrop && this.ContainsMouse() &&
                   !AnyChildIsDropCandidate(toDrop, c => c?.Control_MouseMove(sender, e))) {
                    if(_leftTreeContent != toDrop)
                        Highlight(_leftTreeContent, () => Mouse.GetPosition(AndOr).Y <= AndOr.ActualHeight / 2, toDrop);
                    if(_rightTreeContent != toDrop)
                        Highlight(_rightTreeContent, () => Mouse.GetPosition(AndOr).Y > AndOr.ActualHeight / 2, toDrop);
                }
            }
        }

        private void Highlight(ResetConditionTreeView treeView, Func<bool> onRightSide, ResetConditionTreeView toDrop) {
            bool ShouldHighlight() => this.ContainsMouse() && onRightSide() && !AnyChildIsDropCandidate(toDrop) &&
                                      Mouse.LeftButton == MouseButtonState.Pressed;

            if(!ShouldHighlight()) return;

            treeView.AdornerRectangle.Visibility = Visibility.Visible;
            treeView.AdornerRectangle.Width = treeView.ActualWidth;
            treeView.AdornerRectangle.Height = treeView.ActualHeight;

            if(treeView._unhighlightTimer is {IsEnabled: true}) return;

            treeView._unhighlightTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(10)};
            treeView._unhighlightTimer.Tick += (_,  _) => {
                if(!ShouldHighlight()) {
                    treeView.AdornerRectangle.Visibility = Visibility.Collapsed;
                    treeView._unhighlightTimer.Stop();
                }
            };
            treeView._unhighlightTimer.Start();
        }

        private void AnimateOpacity(double to) {
            var animation = new DoubleAnimation {
                From = Opacity, To = to, Duration = Duration
            };
            Storyboard.SetTargetName(animation, Root.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
            _opacityStoryboard.Children.Clear();
            _opacityStoryboard.Children.Add(animation);
            _opacityStoryboard.Begin(this);
        }
    }
}