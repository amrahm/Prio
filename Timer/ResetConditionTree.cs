using System;
using Infrastructure.SharedResources;

namespace Timer {
    /// <summary> A Boolean-Algebra-Tree that can evaluate if a set of ResetConditions are met </summary>
    [Serializable]
    public class ResetConditionTree : NotifyPropertyChanged {
        /// <summary> The condition to evaluate. This must be null if this tree is not a leaf </summary>
        public ResetCondition Condition { get; set; }

        /// <summary> The left branch. This must be null if this tree is a leaf </summary>
        public ResetConditionTree Left { get; set; }

        /// <summary> The right branch. This must be null if this tree is a leaf </summary>
        public ResetConditionTree Right { get; set; }

        /// <summary> If this is a split, is it an AND? If not, it's an OR </summary>
        public bool IsAnd { get; set; }

        public bool IsSat() => Condition?.IsSatisfied() ??
                               (IsAnd ? Left.IsSat() && Right.IsSat() : Left.IsSat() || Right.IsSat());

        public ResetConditionTree() { }

        public ResetConditionTree(ResetCondition condition) {
            Condition = condition;
        }

        public ResetConditionTree(ResetConditionTree left, ResetConditionTree right, bool isAnd) {
            Left = left;
            Right = right;
            IsAnd = isAnd;
        }

        public ResetConditionTree AddCondition(ResetCondition condition, bool isAnd = true) {
            return Condition != null || Left != null ?
                           new ResetConditionTree(this, new ResetConditionTree(condition), isAnd) :
                           new ResetConditionTree(condition);
        }

        //public void MoveNode(IResetConditionNode node) { }
    }
}