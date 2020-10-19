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
        private double _ctrlWidth = 600;

        public double CtrlWidth {
            get => _ctrlWidth;
            set {
                _ctrlWidth = value;
                OnPropertyChanged();
            }
        }
        private double _masonWidth = 605;

        public double MasonWidth {
            get => _masonWidth;
            set {
                _masonWidth = value;
                OnPropertyChanged();
            }
        }

        public TimerSettingsView() {
            InitializeComponent();
            SizeChanged += SettingsBoxWidthAdjust;
        }

        private void SettingsBoxWidthAdjust(object sender, SizeChangedEventArgs sizeChangedEventArgs) {
            double newSizeWidth = sizeChangedEventArgs.NewSize.Width;
            CtrlWidth = newSizeWidth / Math.Floor(newSizeWidth / 450);
            MasonWidth = newSizeWidth + 7;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}