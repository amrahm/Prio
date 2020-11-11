using System;
using Infrastructure.SharedResources;

namespace Timer {
    [Serializable]
    public class TimersGeneralConfig : NotifyPropertyChanged {
        public ShortcutDefinition ShowHideTimersShortcut { get; set; }
        public ShortcutDefinition KeepTimersOnTopShortcut { get; set; }
        public ShortcutDefinition MoveTimersBehindShortcut { get; set; }
        public VisibilityState DefaultVisibilityState { get; set; }
    }

    public enum VisibilityState { KeepOnTop, MoveBehind, Hidden }
}