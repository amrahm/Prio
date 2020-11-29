using System;
using System.Collections.Generic;
using Infrastructure.SharedResources;
using WeakEvent;

namespace Timer {
    public enum ResetConditionType { Cooldown, Dependency }

    [Serializable]
    public class ResetCondition : NotifyPropertyChanged {
        public Guid TimerId { get; set; }
        public ResetConditionType Type { get; set; }
        public bool AllowOverride { get; set; }

        public int WaitForMinutes { get; set; }
        public bool OffDesktopsEnabled { get; set; }
        public HashSet<int> OffDesktopsSet { get; set; }

        public Guid DependencyTimerId { get; set; }
        public bool MustRunForXEnabled { get; set; }
        public int MustRunForXMinutes { get; set; }
        public bool MustBeFinished { get; set; }


        private readonly WeakEventSource<EventArgs> _deleteRequested = new WeakEventSource<EventArgs>();

        public event EventHandler<EventArgs> DeleteRequested {
            add => _deleteRequested.Subscribe(value);
            remove => _deleteRequested.Unsubscribe(value);
        }

        public virtual void DeleteMe() {
            _deleteRequested.Raise(this, EventArgs.Empty);
        }

        public bool IsSatisfied() => AllowOverride;

        public ResetCondition(ITimer timer) {
            if(timer != null) TimerId = timer.Config.InstanceID;
        }
    }
}