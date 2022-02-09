using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Infrastructure.SharedResources;

namespace Timer {
    [Serializable]
    public class TimersGeneralConfig : NotifyPropertyChanged {
        public ShortcutDefinition ToggleVisibilityShortcut { get; set; }
        public ShortcutDefinition KeepTimersOnTopShortcut { get; set; }
        public ShortcutDefinition MoveTimersBehindShortcut { get; set; }
        public ShortcutDefinition StopAllShortcut { get; set; }
        public ShortcutDefinition ResumeAllShortcut { get; set; }
        public Dictionary<int, VisibilityState> VisStatePerDesktopProfile { get; set; } = new();
        public VisibilityState VisState {
            get => VisStatePerDesktopProfile.GetValueOrDefault(Screen.AllScreens.Count(), VisibilityState.KeepOnTop);
            set => VisStatePerDesktopProfile[Screen.AllScreens.Count()] = value;
        }
        public IList<TimerConfig> TimerConfigs { get; set; } = new List<TimerConfig>();
    }

    public enum VisibilityState { KeepOnTop, MoveBehind, Hidden }
}