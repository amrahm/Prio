using System;
using System.Runtime.Serialization;
using Infrastructure.SharedResources;
using JetBrains.Annotations;

namespace Timer {
    /// <summary> A Boolean-Algebra-Tree that can evaluate if a set of ResetConditions are met </summary>
    [Serializable]
    public class ResetConditionTree : NotifyPropertyChanged {
        /// <summary> The condition to evaluate. This must be null if this tree is not a leaf </summary>
        public ResetCondition Condition { get; set; }

        /// <summary> The left branch. This must be null if this tree is a leaf </summary>
        public ResetConditionTree Left {
            get => _left;
            set => _left = value;
        }

        /// <summary> The right branch. This must be null if this tree is a leaf </summary>
        public ResetConditionTree Right {
            get => _right;
            set => _right = value;
        }

        /// <summary> If this is a branch split, is it an AND? If not, it's an OR </summary>
        public bool IsAnd { get; set; }

        public bool IsLeaf => Left == null;

        public bool IsSat() => Condition?.IsSatisfied() ??
                               (IsAnd ? Left.IsSat() && Right.IsSat() : Left.IsSat() || Right.IsSat());

        internal ResetConditionTree parent;
        private ResetConditionTree _left;
        private ResetConditionTree _right;

        public ResetConditionTree() { }

        public ResetConditionTree(ResetCondition condition) {
            Condition = condition;
        }

        public ResetConditionTree([NotNull] ResetConditionTree left, ResetConditionTree right, bool isAnd) {
            Left = left;
            Right = right;
            IsAnd = isAnd;

            SetParents();
        }

        private void SetParents() {
            if(IsLeaf) return;
            Left.parent = this;
            Right.parent = this;
        }

        [OnDeserialized]
        private void SetParents(StreamingContext streamingContext) => SetParents();

        public void AddCondition(ResetCondition newCondition, bool isAnd = true) =>
                AddCondition(new ResetConditionTree(newCondition), isAnd);

        public void AddCondition(ResetConditionTree newConditionTree, bool isAnd = true, bool toLeft = true) {
            if(Condition == null && Left == null) {
                if(newConditionTree.IsLeaf) {
                    Condition = newConditionTree.Condition;
                } else {
                    _left = newConditionTree._left;
                    _right = newConditionTree._right;
                }
            } else {
                GetDir(!toLeft) = IsLeaf ? new ResetConditionTree(Condition) : new ResetConditionTree(_left, _right, isAnd);
                GetDir(toLeft) = newConditionTree;
                IsAnd = isAnd;
                Condition = null;
            }
            SetParents();
        }

        private ref ResetConditionTree GetDir(bool left) {
            if(left) return ref _left;
            return ref _right;
        }

        public void MoveNode(ResetConditionTree tree, bool toLeft, bool toLeftOfAdded) {
            bool fromDir = tree.parent._left == tree;
            if(this == tree.parent && fromDir == toLeft) return;
            if(this == tree.parent && tree.parent.GetDir(!fromDir).IsLeaf) {
                tree.parent.GetDir(fromDir) = tree.parent.GetDir(!fromDir);
                tree.parent.GetDir(!fromDir) = tree;
            } else {
                if(tree.parent.GetDir(!fromDir).IsLeaf) {
                    tree.parent.Condition = tree.parent.GetDir(!fromDir).Condition;
                    tree.parent._left = tree.parent._right = null;
                } else {
                    tree.parent.GetDir(fromDir) = tree.parent.GetDir(!fromDir).GetDir(fromDir);
                    tree.parent.GetDir(!fromDir) = tree.parent.GetDir(!fromDir).GetDir(!fromDir);
                    tree.parent.SetParents();
                }
                GetDir(toLeft).AddCondition(tree, true, toLeftOfAdded);
            }

            Root().OnPropertyChanged();
        }

        public ResetConditionTree Root() {
            ResetConditionTree root = this;
            while(root.parent != null) root = root.parent;
            return root;
        }

        private string _PrintPretty(string indent = "") {
            string rep = indent;
            rep += IsLeaf ? $"{Condition?.Type}\n" : "<\n";

            rep += Left?._PrintPretty(indent + "|");
            rep += Right?._PrintPretty(indent + "|");
            return rep;
        }

        public override string ToString() => _PrintPretty();
    }
}