using System;
using System.Collections.Generic;
using System.Windows.Input;
using Prism.Mvvm;

namespace Timer {
    [Serializable]
    public class TimerConfig : BindableBase {
        public Guid InstanceID { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public bool ShowName { get; set; } = true;
        public TimeSpan Duration { get; set; } = TimeSpan.FromHours(1);
        public TimeSpan TimeLeft { get; set; } = TimeSpan.FromHours(1);
        public IList<int> DesktopsVisible { get; set; }
        public IList<int> DesktopsActive { get; set; }
        public Shortcut ResetShortcut { get; set; }
        public Shortcut StartShortcut { get; set; }
        public Shortcut StopShortcut { get; set; }
    }
}