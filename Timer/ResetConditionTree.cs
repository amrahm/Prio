using System;
using System.Runtime.Serialization;
using Infrastructure.SharedResources;
using JetBrains.Annotations;

namespace Timer {
    /// <summary> A Boolean-Algebra-Tree that can evaluate if a set of ResetConditions are met </summary>
    [Serializable]
    public class ResetConditionTree : NotifyPropertyChanged {
        /// <summary> The condition to evaluate. This must be null if this tree is not a leaf </summary>
        public ResetCondition Condition {
            get => _condition;
            set {
                //if(_condition != null) _condition.DeleteRequested -= ConditionOnDeleteRequested;
                _condition = value;
                if(_condition != null) _condition.DeleteRequested += ConditionOnDeleteRequested;
            }
        }

        private void ConditionOnDeleteRequested(object sender, EventArgs eventArgs) {
            Condition = null;
            _parent?._DeleteChild(IsLeftChild);
            Root().OnPropertyChanged();
        }

        private void _DeleteChild(bool fromDir) {
            if(GetDir(!fromDir).IsLeaf) {
                Condition = GetDir(!fromDir).Condition;
                _left = _right = null;
            } else {
                GetDir(fromDir) = GetDir(!fromDir).GetDir(fromDir);
                GetDir(!fromDir) = GetDir(!fromDir).GetDir(!fromDir);
                SetParentOfChildren();
            }
        }

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
                               (IsAnd ? _left.IsSat() && _right.IsSat() : _left.IsSat() || _right.IsSat());

        private ResetConditionTree _parent;
        private ResetConditionTree _left;
        private ResetConditionTree _right;
        private ResetCondition _condition;

        private bool IsLeftChild => _parent != null && _parent._left == this;

        public ResetConditionTree() { }

        public ResetConditionTree(ResetCondition condition) {
            Condition = condition;
        }

        public ResetConditionTree([NotNull] ResetConditionTree left, ResetConditionTree right, bool isAnd) {
            _left = left;
            _right = right;
            IsAnd = isAnd;

            SetParentOfChildren();
        }

        private void SetParentOfChildren() {
            if(IsLeaf) return;
            _left._parent = this;
            _right._parent = this;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext streamingContext) {
            Condition = _condition;
            SetParentOfChildren();
        }

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
            SetParentOfChildren();
            Root().OnPropertyChanged();
        }

        private ref ResetConditionTree GetDir(bool left) {
            if(left) return ref _left;
            return ref _right;
        }

        public void MoveNode(ResetConditionTree tree, bool toLeft, bool toLeftOfAdded) {
            bool fromDir = tree.IsLeftChild;
            if(this == tree._parent && fromDir == toLeft) return;
            if(this == tree._parent && tree._parent.GetDir(!fromDir).IsLeaf) {
                tree._parent.GetDir(fromDir) = tree._parent.GetDir(!fromDir);
                tree._parent.GetDir(!fromDir) = tree;
            } else {
                tree._parent._DeleteChild(fromDir);
                GetDir(toLeft).AddCondition(tree, true, toLeftOfAdded);
            }

            Root().OnPropertyChanged();
        }

        public ResetConditionTree Root() {
            ResetConditionTree root = this;
            while(root._parent != null) root = root._parent;
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