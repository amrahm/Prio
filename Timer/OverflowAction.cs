using System;
using System.Windows.Media;
using Infrastructure.SharedResources;

namespace Timer {
    [Serializable]
    public class OverflowAction : NotifyPropertyChanged {
        public double AfterMinutes { get; set; }
        public bool FlashColorEnabled { get; set; }
        public Brush FlashColor { get; set; } = Brushes.Crimson;
        public int FlashColorSeconds { get; set; }
        public bool PlaySoundEnabled { get; set; }
        public string PlaySoundFile { get; set; }
        public bool ShowMessageEnabled { get; set; }

        public event EventHandler<EventArgs> DeleteRequested;
        public void DeleteMe() => DeleteRequested?.Invoke(this, EventArgs.Empty);
    }
}