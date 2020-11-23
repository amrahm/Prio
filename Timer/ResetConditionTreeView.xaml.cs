using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public List<UIElement> ChildDraggables { get; } = new List<UIElement>();
        private readonly ResetConditionTreeViewModel _vm;
        private readonly Storyboard _opacityStoryboard = new Storyboard();
        private  DispatcherTimer _unhighlightTimer;
        private  bool _isRoot;

        public ResetConditionTreeView(ResetConditionTreeViewModel vm) : this(vm, Color.Lavender) { }

        private ResetConditionTreeView(ResetConditionTreeViewModel vm, Color bgColor) {
            _vm = vm;
            _bgColor = bgColor;
            DataContext = _vm;
            InitializeComponent();

            Loaded += (sender,  args) => {
                this.InitializeDraggable();

                if(this.TryFindAncestor<IDraggable>() is TimerSettingsView settings) {
                    _isRoot = true;
                    Background = Brushes.Transparent;
                    settings.ChildDraggables = ChildDraggables;

                    _vm.PropertyChanged += (o,  e) => {
                        if(e.PropertyName == nameof(_vm.Tree)) {
                            ChildDraggables.Clear();
                            UpdateTreeLayout();
                        }
                    };
                }
            };
            UpdateTreeLayout();

            PreviewMouseLeftButtonUp += Control_MouseLeftButtonUpPreview;
            MouseLeftButtonUp += Control_MouseLeftButtonUp;
            MouseMove += Control_MouseMove;
        }

        private void UpdateTreeLayout() {
            if(_vm.Tree.IsLeaf) {
                if(_vm.Tree.Condition != null)
                    LeftTree.Content = new ResetConditionView(new ResetConditionViewModel(_vm.Tree.Condition));

                Background = Brushes.Transparent;
                AndOr.Visibility = Visibility.Collapsed;
                RightTree.Visibility = Visibility.Collapsed;
            } else {
                var leftTreeContent = new ResetConditionTreeView(new ResetConditionTreeViewModel(_vm.Tree.Left),
                                                                 _bgColor.Rotate(ROTATION));
                var rightTreeContent = new ResetConditionTreeView(new ResetConditionTreeViewModel(_vm.Tree.Right),
                                                                  _bgColor.Rotate(ROTATION));
                LeftTree.Content = leftTreeContent;
                RightTree.Content = rightTreeContent;
                _leftTreeContent = leftTreeContent;
                _rightTreeContent = rightTreeContent;

                if(!_isRoot) Background = new SolidColorBrush(_bgColor.ToMediaColor());
                AndOr.Visibility = Visibility.Visible;
                RightTree.Visibility = Visibility.Visible;
            }
        }

        private bool _isDragging;
        private Point _initPosition;
        private readonly Color _bgColor;
        private ResetConditionTreeView _leftTreeContent;
        private ResetConditionTreeView _rightTreeContent;

        private void Control_MouseLeftButtonUpPreview(object sender, MouseEventArgs e) {
            if(_isDragging || _vm.Tree.IsLeaf) return;
            if(Mouse.Captured is ResetConditionTreeView dropped &&
               !AnyChildIsDropCandidate(dropped, c => c.Control_MouseLeftButtonUpPreview(sender, e))) {
                bool toLeft = e.GetPosition(AndOr).Y < AndOr.ActualHeight / 2;
                FrameworkElement addingTo = toLeft ? _leftTreeContent : _rightTreeContent;
                bool toLeftOfAdded = e.GetPosition(addingTo).Y < addingTo.ActualHeight / 2;
                _vm.Tree.MoveNode(dropped._vm.Tree, toLeft, toLeftOfAdded);
                dropped.Control_MouseLeftButtonUp(dropped, e);
            }
        }

        /// <summary> Checks if any child is under the mouse, since they should get priority.
        ///           If they are, call callback with them as a parameter </summary>
        private bool AnyChildIsDropCandidate(ResetConditionTreeView dropped,
                                             Action<ResetConditionTreeView> callback = null) => ChildDraggables.Any(
                delegate(UIElement x) {
                    if(x != dropped && x is ResetConditionTreeView c && c.ContainsMouse() && !c._vm.Tree.IsLeaf) {
                        callback?.Invoke(c);
                        return true;
                    }
                    return false;
                });

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
            if(e.LeftButton != MouseButtonState.Pressed || !(sender is ResetConditionTreeView draggable)) return;

            if(!_isDragging && Mouse.Captured == null && !ChildDraggables.Any(x => x.IsMouseOver) && !_isRoot) {
                _isDragging = true;
                _initPosition = e.GetPosition(Parent as UIElement);
                draggable.SetGlobalZ(int.MaxValue);
                draggable.CaptureMouse();
                AnimateOpacity(.6);
            }

            if(_isDragging) {
                Point currentPosition = e.GetPosition(Parent as UIElement);
                if(!(draggable.RenderTransform is TranslateTransform transform)) {
                    transform = new TranslateTransform();
                    draggable.RenderTransform = transform;
                }
                transform.X = currentPosition.X - _initPosition.X;
                transform.Y = currentPosition.Y - _initPosition.Y;
            } else if(!_vm.Tree.IsLeaf) {
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

            if(treeView._unhighlightTimer != null && treeView._unhighlightTimer.IsEnabled) return;

            treeView._unhighlightTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(10)};
            treeView._unhighlightTimer.Tick += (o,  args) => {
                if(!ShouldHighlight()) {
                    treeView.AdornerRectangle.Visibility = Visibility.Collapsed;
                    treeView._unhighlightTimer.Stop();
                }
            };
            treeView._unhighlightTimer.Start();
        }

        private void AnimateOpacity(double to) {
            var animation = new DoubleAnimation {
                From = Opacity, To = to, Duration = new Duration(TimeSpan.FromMilliseconds(200))
            };
            Storyboard.SetTargetName(animation, Root.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));
            _opacityStoryboard.Children.Clear();
            _opacityStoryboard.Children.Add(animation);
            _opacityStoryboard.Begin(this);
        }
    }
}