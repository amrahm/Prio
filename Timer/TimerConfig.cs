using System;
using System.Collections.Generic;
using Prism.Mvvm;

namespace Timer {
    public class TimerConfig : BindableBase {
        public string Name { get; set; }
        public bool ShowName { get; set; } = true;
        public TimeSpan Duration { get; set; }
        public IList<int> DesktopsVisible { get; set; }
        public IList<int> DesktopsActive { get; set; }
    }
}