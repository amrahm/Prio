using System;
using System.Collections.Generic;
using Infrastructure.SharedResources;
using WeakEvent;

namespace Timer {
    public enum ResetConditionType { Cooldown, Dependency }

    [Serializable]
    public class ResetCondition : NotifyPropertyChanged {
        public ResetConditionType Type { get; set; }
        public bool AllowOverride { get; set; }
        public int WaitForMinutes { get; set; }
        public bool OffDesktopsEnabled { get; set; }
        public HashSet<int> OffDesktopsSet { get; set; }


        private readonly WeakEventSource<EventArgs> _deleteRequested = new WeakEventSource<EventArgs>();

        public event EventHandler<EventArgs> DeleteRequested {
            add => _deleteRequested.Subscribe(value);
            remove => _deleteRequested.Unsubscribe(value);
        }

        public virtual void DeleteMe() {
            _deleteRequested.Raise(this, EventArgs.Empty);
        }

        public bool IsSatisfied() => AllowOverride;
    }
}