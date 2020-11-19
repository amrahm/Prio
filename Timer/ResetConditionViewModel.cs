using System.Collections.Generic;
using Infrastructure.SharedResources;
using static Infrastructure.SharedResources.VirtualDesktopExtensions;

namespace Timer {
    public enum ResetConditionType { Cooldown, Dependency }

    public class ResetConditionViewModel : NotifyPropertyChanged {
        public ResetConditionType Type { get; set; }
        public int WaitForMinutes { get; set; }
        public bool OffDesktopsEnabled { get; set; }

        public string OffDesktopsConverter {
            get => DesktopSetToString(OffDesktopsSet);
            set => OffDesktopsSet = DesktopStringToSet(value);
        }

        public HashSet<int> OffDesktopsSet { get; set; }

        public bool AllowOverride { get; set; }
    }
}