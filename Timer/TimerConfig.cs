using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Prism.Mvvm;
using Timer.Annotations;

namespace Timer {
    public class TimerConfig : BindableBase {
        public Guid InstanceID { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public bool ShowName { get; set; } = true;
        public TimeSpan Duration { get; set; } = TimeSpan.FromHours(1);
        public TimeSpan TimeLeft { get; set; } = TimeSpan.FromHours(1);
        public IList<int> DesktopsVisible { get; set; } = new List<int> {0};
        public IList<int> DesktopsActive { get; set; } = new List<int> {0};
    }
}