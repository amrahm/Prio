using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using TimerSettings.Annotations;

namespace TimerSettings
{
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

        public TimerSettingsView()
        {
            InitializeComponent();
            SizeChanged += TimerAspectRatioLimits;
        }

        private void TimerAspectRatioLimits(object sender, SizeChangedEventArgs sizeChangedEventArgs) {
            double newSizeWidth = sizeChangedEventArgs.NewSize.Width;
            CtrlWidth = newSizeWidth < 900 ? newSizeWidth : newSizeWidth / 2;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
