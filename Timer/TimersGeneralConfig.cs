using System;
using Infrastructure.SharedResources;
using Prism.Mvvm;

namespace Timer {
    [Serializable]
    public class TimersGeneralConfig : BindableBase {
        public ShortcutDefinition ShowHideShortcut { get; set; }
        public ShortcutDefinition KeepOnTopShortcut { get; set; }
        public ShortcutDefinition MoveBehindShortcut { get; set; }
    }
}