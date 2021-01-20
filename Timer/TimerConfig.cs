using System;
using System.Collections.Generic;
using System.Windows.Media;
using Infrastructure.SharedResources;
using static Infrastructure.SharedResources.ColorUtil;

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
        public ShortcutDefinition ToggleVisibilityShortcut { get; set; }
        public Dictionary<int, WindowPosition> WindowPositions { get; set; } = new();
        public ResetConditionTree ResetConditions { get; set; } = new();
        public bool AutoResetOnConditions { get; set; }
        public bool AllowResetOverride { get; set; }
        public bool AllowResetWhileRunning { get; set; }
        public bool OverflowEnabled { get; set; } = true;
        public OverflowAction ZeroOverflowAction { get; set; }
        public List<OverflowAction> OverflowActions { get; set; } = new();
        public SolidColorBrush BackgroundColor { get; set; } = new(FromHex("#184A8C").ToMediaColor());
        public SolidColorBrush TextColor { get; set; } = new(FromHex("#F26F63").ToMediaColor());
        public SolidColorBrush NameBackgroundColor { get; set; } = new(FromHex("#7A90AC").ToMediaColor());
        public SolidColorBrush NameTextColor { get; set; } = new(FromHex("#BEF2DC").ToMediaColor());
        public SolidColorBrush DividerColor { get; set; } = new(FromHex("#f8f2d7").ToMediaColor());
        public bool LockedPauseEnabled { get; set; } = true;
        public bool InactivityPauseEnabled { get; set; } = true;
        private double _inactivityMinutes = 2;
        public double InactivityMinutes {
            get => _inactivityMinutes;
            set => _inactivityMinutes = Math.Max(value, .1);
        }
        public bool DailyResetEnabled { get; set; }
        public DateTime DailyResetTime { get; set; }


        public TimerConfig() {
            ZeroOverflowAction ??= new OverflowAction(InstanceID);
        }

        public void SetColor(SolidColorBrush color, TimerColorZone zone) {
            switch(zone) {
                case TimerColorZone.Background:
                    BackgroundColor = color;
                    break;
                case TimerColorZone.Text:
                    TextColor = color;
                    break;
                case TimerColorZone.NameBackground:
                    NameBackgroundColor = color;
                    break;
                case TimerColorZone.NameText:
                    NameTextColor = color;
                    break;
                case TimerColorZone.Divider:
                    DividerColor = color;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(zone), zone, null);
            }
        }

        public SolidColorBrush GetColor(TimerColorZone zone) {
            return zone switch {
                TimerColorZone.Background => BackgroundColor,
                TimerColorZone.Text => TextColor,
                TimerColorZone.NameBackground => NameBackgroundColor,
                TimerColorZone.NameText => NameTextColor,
                TimerColorZone.Divider => DividerColor,
                _ => throw new ArgumentOutOfRangeException(nameof(zone), zone, null)
            };
        }
    }

    public enum TimerColorZone { Background, Text, NameBackground, NameText, Divider }

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

        private bool Equals(WindowPosition other) => X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) &&
                                                     Height.Equals(other.Height);

        public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

        public static bool operator ==(WindowPosition left, WindowPosition right) => left.Equals(right);

        public static bool operator !=(WindowPosition left, WindowPosition right) => !(left == right);
    }
}