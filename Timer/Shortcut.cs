using System.Collections.Generic;
using System.Windows.Input;

namespace Timer {
    public class Shortcut {
        public Key key;
        public ISet<Key> modifiers = new SortedSet<Key>();

        public override string ToString() {
            return $"{string.Join("+", modifiers)}{(modifiers.Count == 0 ? "" : "+")}{key}";
        }
    }
}