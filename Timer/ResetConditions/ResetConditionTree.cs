using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Infrastructure.SharedResources;
using Newtonsoft.Json;
using WeakEvent;

namespace Timer {
    /// <summary> A Boolean-Algebra-Tree that can evaluate if a set of ResetConditions are met </summary>
    [Serializable]
    public class ResetConditionTree : NotifyPropertyChanged {
        private readonly WeakEventSource<EventArgs> _satisfied = new();
        public event EventHandler<EventArgs> Satisfied {
            add => _satisfied.Subscribe(value);
            remove => _satisfied.Unsubscribe(value);
        }

        /// <summary> The condition to evaluate. This must be null if this node is not a leaf </summary>
        public ResetCondition Condition {
            get => _condition;
            set {
                if(_condition != null) {
                    _condition.Satisfied -= ConditionOnSatisfied;
                    _condition.DeleteRequested -= ConditionOnDeleteRequested;
                }
                _condition = value;
                if(_condition != null) {
                    _condition.Satisfied += ConditionOnSatisfied;
                    _condition.DeleteRequested += ConditionOnDeleteRequested;
                }
            }
        }

        private void ConditionOnSatisfied(object sender, EventArgs e) {
            if(_parent == null) {
                _satisfied.Raise(this, EventArgs.Empty);
                StopAndResetAllConditions();
            } else if(!_parent.IsAnd || _parent.GetDir(!IsLeftChild).IsSat()) _parent.ConditionOnSatisfied(sender, e);
        }

        public void StopAndResetAllConditions() {
            if(IsLeaf) Condition.StopAndReset();
            else if(IsBranch) {
                Left.StopAndResetAllConditions();
                Right.StopAndResetAllConditions();
            }
        }

        private void ConditionOnDeleteRequested(object sender, EventArgs eventArgs) {
            Condition = null;
            ResetConditionTree root = Root();
            _parent?._DeleteChild(IsLeftChild);
            root.OnPropertyChanged();
        }

        /// <summary> The left branch. This must be null if this node is a leaf </summary>
        public ResetConditionTree Left {
            get => _left;
            set => _left = value;
        }

        /// <summary> The right branch. This must be null if this node is a leaf </summary>
        public ResetConditionTree Right {
            get => _right;
            set => _right = value;
        }

        /// <summary> If this is a branch split, is it an AND? If not, it's an OR </summary>
        public bool IsAnd { get; set; }

        [JsonIgnore] private bool IsLeaf => Condition != null;
        [JsonIgnore] public bool IsBranch => Left != null;

        public bool IsSat() {
            if(IsLeaf) return Condition.IsSatisfied() ?? _parent == null || _parent.IsAnd;
            if(IsBranch) return IsAnd ? _left.IsSat() && _right.IsSat() : _left.IsSat() || _right.IsSat();
            return true;
        }

        public void StartConditions() {
            if(IsLeaf) Condition.Start();
            else if(IsBranch) {
                Left.StartConditions();
                Right.StartConditions();
            }
        }

        public string UnmetStrings() {
            if(IsSat()) return "";
            string st = Left.IsSat() ? Right._UnmetStrings() : Right.IsSat() ? Left._UnmetStrings() : _UnmetStrings();
            return _parent == null ? st.TrimEnd('\n') : st;
        }


        private string _UnmetStrings(string indent = "", bool init = true) {
            if(IsLeaf) return $"{indent}{Condition.UnmetString()}\n";
            if(!IsBranch) return "";

            string rep = indent;
            if(!init) indent += "  ";

            if(!Left.IsSat()) {
                rep += Left._UnmetStrings(indent, false);
                rep += $"{indent}{(IsAnd ? "and" : "or")}\n";
            }
            if(!Right.IsSat())
                rep += Right._UnmetStrings(indent, false);
            return rep;
        }


        private ResetConditionTree _parent;
        private ResetConditionTree _left;
        private ResetConditionTree _right;
        private ResetCondition _condition;

        private bool IsLeftChild => _parent != null && _parent._left == this;

        public ResetConditionTree() { }

        public ResetConditionTree(ResetCondition condition) {
            Condition = condition;
        }

        public ResetConditionTree(ResetConditionTree left, ResetConditionTree right, bool isAnd) {
            _left = left;
            _right = right;
            IsAnd = isAnd;

            SetParentOfChildren();
        }

        private void SetParentOfChildren() {
            if(!IsBranch) return;
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
                if(newConditionTree.IsBranch) {
                    _left = newConditionTree._left;
                    _right = newConditionTree._right;
                } else {
                    Condition = newConditionTree.Condition;
                }
            } else {
                GetDir(toLeft) = IsBranch ? new ResetConditionTree(_left, _right, IsAnd) : new ResetConditionTree(Condition);
                GetDir(!toLeft) = newConditionTree;
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

        private void _DeleteChild(bool fromDir) {
            if(GetDir(!fromDir).IsBranch) {
                GetDir(fromDir) = GetDir(!fromDir).GetDir(fromDir);
                GetDir(!fromDir) = GetDir(!fromDir).GetDir(!fromDir);
                SetParentOfChildren();
            } else {
                Condition = GetDir(!fromDir).Condition;
                GetDir(!fromDir).Condition = null;
                _left = _right = null;
            }
        }

        /// <summary> Moves the node "node" from wherever it is to this branch, appending it to the child specified by "toLeft" </summary>
        /// <param name="node"> The node being moved </param>
        /// <param name="toLeft"> The direction on this tree that the node is being added to </param>
        /// <param name="toLeftOfAdded"> When we add it to that node, should we put the moved node on
        ///                              the left or right of the child currently at "toLeft" </param>
        public void MoveNode(ResetConditionTree node, bool toLeft, bool toLeftOfAdded) {
            bool fromDir = node.IsLeftChild;
            if(this == node._parent && fromDir == toLeft) return;
            ResetConditionTree sibling = node._parent.GetDir(!fromDir);
            if(this == node._parent && sibling.IsLeaf) {
                node._parent.GetDir(fromDir) = sibling;
                node._parent.GetDir(!fromDir) = node;
            } else {
                node._parent._DeleteChild(fromDir);
                GetDir(toLeft).AddCondition(node, true, toLeftOfAdded);
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
            rep += IsBranch ? "<\n" : $"{Condition?.Type}\n";

            rep += Left?._PrintPretty(indent + "|");
            rep += Right?._PrintPretty(indent + "|");
            return rep;
        }

        public override string ToString() => _PrintPretty();

        public IEnumerator<ResetCondition> GetEnumerator() {
            if(IsLeaf) yield return Condition;
            if(IsBranch) {
                foreach(ResetCondition condition in _left) yield return condition;
                foreach(ResetCondition condition in _right) yield return condition;
            }
        }
    }
}