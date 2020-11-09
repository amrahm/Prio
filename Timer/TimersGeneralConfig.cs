using System;
using Infrastructure.SharedResources;
using Prism.Mvvm;

namespace Timer {
    [Serializable]
    public class TimersGeneralConfig : BindableBase {
        public ShortcutDefinition ShowHideTimersShortcut { get; set; }
        public ShortcutDefinition KeepTimersOnTopShortcut { get; set; }
        public ShortcutDefinition MoveTimersBehindShortcut { get; set; }
    }
}