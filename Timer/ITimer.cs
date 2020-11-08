using System;
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
        public event Action RequestHide;
        public event Action RequestKeepOnTop;
        public event Action RequestMoveBelow;
    }
}