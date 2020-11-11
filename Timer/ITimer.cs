using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows;
using JetBrains.Annotations;

namespace Timer {
    public interface ITimer : INotifyPropertyChanged {
        TimerConfig Config { get; set; }
        void ShowTimer();
        void StartTimer();
        void StopTimer();
        [JsonIgnore] bool IsRunning { get; }
        void OpenSettings();
        void SaveSettings();
        [CanBeNull] public Window TimerWindow { get; set; }
    }
}