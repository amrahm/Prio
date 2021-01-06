using System.ComponentModel;
using Infrastructure.SharedResources;
using JetBrains.Annotations;

namespace Timer {
    public enum BooleanType { [Description("AND")] And, [Description("OR")] [UsedImplicitly] Or }

    public class ResetConditionTreeViewModel : NotifyPropertyWithDependencies {
        private ResetConditionTree _tree;

        public ResetConditionTree Tree {
            get => _tree;
            set => NotificationBubbler.BubbleSetter(ref _tree, value, (o, e) => this.OnPropertyChanged());
        }

        [DependsOnProperty(nameof(Tree))]
        public BooleanType IsAnd {
            get => Tree.IsAnd ? BooleanType.And : BooleanType.Or;
            set => Tree.IsAnd = value == BooleanType.And;
        }

        public ResetConditionTreeViewModel(ResetConditionTree tree) => Tree = tree;
    }
}