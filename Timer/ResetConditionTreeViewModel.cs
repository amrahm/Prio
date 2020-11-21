using System.ComponentModel;
using System.Windows;
using GongSolutions.Wpf.DragDrop;
using Infrastructure.SharedResources;

namespace Timer {
    public enum BooleanType { [Description("AND")] And, [Description("OR")] Or }

    public class ResetConditionTreeViewModel : NotifyPropertyChanged, IDropTarget {
        private ResetConditionTree _tree;
        private BooleanType _isAnd;

        public ResetConditionTree Tree {
            get => _tree;
            set => NotificationBubbler.BubbleSetter(ref _tree, value, (o, e) => OnPropertyChanged());
        }

        public BooleanType IsAnd {
            get => _isAnd;
            set {
                _isAnd = value;
                Tree.IsAnd = value == BooleanType.And;
            }
        }

        public ResetConditionTreeViewModel(ResetConditionTree tree) => Tree = tree;

        public void DragOver(IDropInfo dropInfo) {
            if(dropInfo.Data is ResetConditionTreeView source &&
               dropInfo.TargetItem is ResetConditionTreeView target) {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        public void Drop(IDropInfo dropInfo) {
            ResetConditionTreeViewModel sourceItem = dropInfo.Data as ResetConditionTreeViewModel;
            ResetConditionTreeViewModel targetItem = dropInfo.TargetItem as ResetConditionTreeViewModel;
            //targetItem.Children.Add(sourceItem);
        }
    }
}