using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using TimerSettings.Annotations;

namespace TimersList {
    /// <summary>
    /// Interaction logic for TimersListView.xaml
    /// </summary>
    public partial class TimersListView : INotifyPropertyChanged {
        private const int MinCtrlWidth = 250;
        private double _ctrlWidth = 600;

        public double CtrlWidth {
            get => _ctrlWidth;
            set {
                _ctrlWidth = value;
                OnPropertyChanged();
            }
        }

        public TimersListView() {
            InitializeComponent();
            SizeChanged += SettingsBoxWidthAdjust;
        }

        private void SettingsBoxWidthAdjust(object sender, SizeChangedEventArgs sizeChangedEventArgs) {
            double newSizeWidth = sizeChangedEventArgs.NewSize.Width;
            double maxPerRow = Math.Floor(newSizeWidth / MinCtrlWidth);
            CtrlWidth = newSizeWidth / Math.Min(maxPerRow, TimerWrapPanel.Children.Count) - 10;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}