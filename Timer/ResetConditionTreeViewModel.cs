using System.ComponentModel;
using Infrastructure.SharedResources;
using JetBrains.Annotations;

namespace Timer {
    public enum BooleanType { [Description("AND")] And, [Description("OR")] [UsedImplicitly] Or }

    public class ResetConditionTreeViewModel : NotifyPropertyChanged {
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
    }
}