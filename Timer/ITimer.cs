using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;
using Prism.Dialogs;

namespace Timer {
    public interface ITimer : INotifyPropertyChanged, IDisposable {
        TimerConfig Config { get; set; }
        void ShowTimer(bool shouldActivate = false);
        void SetTopmost();
        void SetBottommost();
        void ToggleLockPosition();
        void SetVisibility(bool vis);
        void ToggleVisibility();
        void ToggleEnabled();
        void RequestResetTimer();
        void StartTimer();
        void StopTimer();
        [JsonIgnore] bool Running { get; }
        Task<ButtonResult> OpenSettings();
        void SaveSettings();
        Brush TempBackgroundBrush { get; set; }
        Brush TempTextBrush { get; set; }
        event EventHandler<EventArgs> Finished;
        void CheckStart();
        void RegisterShortcuts(TimerConfig timerConfig);
        void AddTimerAction(TimerAction action, bool isInsideAction);
        void SetTime(TimeSpan time);
        void AddMinutes(int minutes) => SetTime(Config.TimeLeft + TimeSpan.FromMinutes(minutes));
        void StartStopForDesktopsActive();

        /// <summary> Create a copy of this timer, but with a new instance ID </summary>
        ITimer Duplicate();
    }

    public class TimerAction : IComparable<TimerAction> {
        public TimeSpan TriggerTime { get; }
        public Action Action { get; }

        public TimerAction(TimeSpan triggerTime, Action action) {
            TriggerTime = triggerTime;
            Action = action;
        }

        public int CompareTo(TimerAction other) => -TriggerTime.CompareTo(other.TriggerTime);
    }
}
