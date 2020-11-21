using System;
using System.Collections.Generic;
using Infrastructure.SharedResources;

namespace Timer {
    public enum ResetConditionType { Cooldown, Dependency }

    [Serializable]
    public class ResetCondition : NotifyPropertyChanged {
        public ResetConditionType Type { get; set; }
        public bool AllowOverride { get; set; }
        public int WaitForMinutes { get; set; }
        public bool OffDesktopsEnabled { get; set; }
        public HashSet<int> OffDesktopsSet { get; set; }

        public bool IsSatisfied() => AllowOverride;
    }
}