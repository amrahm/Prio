using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using TimerSettings.Annotations;

namespace TimerSettings {
    /// <summary>
    /// Interaction logic for TimerSettingsView.xaml
    /// </summary>
    public partial class TimerSettingsView : INotifyPropertyChanged {
        private const int MinCtrlWidth = 450;
        private double _ctrlWidth = 600;

        public double CtrlWidth {
            get => _ctrlWidth;
            set {
                _ctrlWidth = value;
                OnPropertyChanged();
            }
        }

        public TimerSettingsView() {
            InitializeComponent();
            SizeChanged += SettingsBoxWidthAdjust;
        }

        private void SettingsBoxWidthAdjust(object sender, SizeChangedEventArgs sizeChangedEventArgs) {
            double newSizeWidth = sizeChangedEventArgs.NewSize.Width;
            double maxPerRow = Math.Floor(newSizeWidth / MinCtrlWidth);
            CtrlWidth = newSizeWidth / Math.Min(maxPerRow, Masonry.Items.Count) -
                        Masonry.Spacing / (double) Masonry.Items.Count + 2;
            Masonry.Width = newSizeWidth + Masonry.Items.Count * Masonry.Spacing + 10;
            Masonry.Spacing = (int) maxPerRow == 1 ? 3 : 5;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}