using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using JetBrains.Annotations;
using Prism.Services.Dialogs;

namespace Timer {
    public interface ITimer : INotifyPropertyChanged {
        TimerConfig Config { get; set; }
        void ShowTimer();
        void StartTimer();
        void StopTimer();
        [JsonIgnore] bool IsRunning { get; }
        Task<ButtonResult> OpenSettings();
        void SaveSettings();
        [CanBeNull] public Window TimerWindow { get; }
    }
}