using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Timer {
    /// <summary> Interaction logic for ResetConditionTreeView.xaml </summary>
    public partial class ResetConditionTreeView {
        private readonly ResetConditionTreeViewModel _vm;
        public bool IsGrey { get; set; }

        public ResetConditionTreeView(ResetConditionTreeViewModel vm) {
            _vm = vm;
            DataContext = _vm;
            InitializeComponent();

            _vm.PropertyChanged += (sender,  args) => {
                if(args.PropertyName == nameof(_vm.Tree)) UpdateTreeLayout();
            };
            UpdateTreeLayout();

            LeftTree.SelectionMode = SelectionMode.Single;
            RightTree.SelectionMode = SelectionMode.Single;

            SizeChanged += (o,  e) => {
                Debug.WriteLine(LeftTree.ActualWidth);
                //foreach(FrameworkElement item in LeftTree.Items) {
                //    item.Width = LeftTree.ActualWidth - 50;
                //}
            };
        }

        private void UpdateTreeLayout() {
            LeftTree.Items.Clear();
            if(_vm.Tree.Condition != null) {
                var item = new ResetConditionView(new ResetConditionViewModel(_vm.Tree.Condition));
                LeftTree.Items.Add(item);
            } else if(_vm.Tree.Left != null) {
                var item = new ResetConditionTreeView(new ResetConditionTreeViewModel(_vm.Tree.Left));
                SetBackground(item);
                LeftTree.Items.Add(item);
            }

            RightTree.Items.Clear();
            if(_vm.Tree.Right != null) {
                AndOr.Visibility = Visibility.Visible;
                RightTree.Visibility = Visibility.Visible;
                ResetConditionTreeView item = new ResetConditionTreeView(new ResetConditionTreeViewModel(_vm.Tree.Right));
                RightTree.Items.Add(item);
                SetBackground(item);
            } else {
                AndOr.Visibility = Visibility.Collapsed;
                RightTree.Visibility = Visibility.Collapsed;
            }
        }

        private void SetBackground(ResetConditionTreeView item) {
            //FIXME this whole thing is broke
            if(IsGrey) {
                item.Background = Brushes.White;
                item.IsGrey = false;
            } else {
                item.Background = Brushes.Gray;
                item.IsGrey = true;
            }
        }
    }
}