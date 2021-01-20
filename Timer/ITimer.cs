using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using JetBrains.Annotations;
using Prism.Services.Dialogs;

namespace Timer {
    public interface ITimer : INotifyPropertyChanged, IDisposable {
        TimerConfig Config { get; set; }
        void ShowTimer();
        void RequestResetTimer();
        void StartTimer();
        void StopTimer();
        [JsonIgnore] bool IsRunning { get; }
        Task<ButtonResult> OpenSettings();
        void SaveSettings();
        [CanBeNull] public Window TimerWindow { get; }
        Brush TempBackgroundBrush { get; set; }
        Brush TempTextBrush { get; set; }
        event EventHandler<EventArgs> Finished;
        void CheckStart();
        void RegisterShortcuts(TimerConfig timerConfig);
        void AddMinutes(int minutes);
    }
}