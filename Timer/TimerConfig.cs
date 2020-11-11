using System;
using System.Collections.Generic;
using Infrastructure.SharedResources;

namespace Timer {
    [Serializable]
    public class TimerConfig : NotifyPropertyChanged {
        public Guid InstanceID { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public bool ShowName { get; set; } = true;
        public bool ShowHours { get; set; } = true;
        public bool ShowMinutes { get; set; } = true;
        public bool ShowSeconds { get; set; } = true;
        public TimeSpan Duration { get; set; } = TimeSpan.FromHours(1);
        public TimeSpan TimeLeft { get; set; } = TimeSpan.FromHours(1);
        public HashSet<int> DesktopsVisible { get; set; }
        public HashSet<int> DesktopsActive { get; set; }
        public ShortcutDefinition ResetShortcut { get; set; }
        public ShortcutDefinition StartShortcut { get; set; }
        public ShortcutDefinition StopShortcut { get; set; }
        public ShortcutDefinition ShowHideShortcut { get; set; }
        public Dictionary<int, WindowPosition> WindowPositions { get; set; } = new Dictionary<int, WindowPosition>();
    }

    public readonly struct WindowPosition {
        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }

        public WindowPosition(double x, double y, double width, double height) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public override bool Equals(object obj) => obj is WindowPosition other && Equals(other);

        public bool Equals(WindowPosition other) =>
            X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);

        public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
    }
}