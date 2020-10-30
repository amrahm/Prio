using System.ComponentModel;

namespace Timer {
    public interface ITimer : INotifyPropertyChanged {
        TimerConfig Config { get; set; }
        void ShowTimer();
        void StartTimer();
        void StopTimer();
        bool IsRunning { get; }
        void OpenSettings();
        void SaveSettings();
    }
}