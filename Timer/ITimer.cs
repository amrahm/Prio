using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;
using Prism.Services.Dialogs;

namespace Timer {
    public interface ITimer : INotifyPropertyChanged, IDisposable {
        TimerConfig Config { get; set; }
        void ShowTimer(bool shouldActivate = false);
        void SetTopmost();
        void SetBottommost();
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
        void SetTime(TimeSpan time);
        void AddMinutes(int minutes) => SetTime(Config.TimeLeft + TimeSpan.FromMinutes(minutes));
        void StartStopForDesktopsActive();
    }
}