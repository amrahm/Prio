namespace Timer {
    public interface ITimer {
        public void ShowTimer();
        public void OpenSettings();
        TimerConfig Config { get; set; }
        void SaveSettings();
    }
}