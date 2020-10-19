using System.Windows;

namespace Timer {
    /// <summary>
    /// Interaction logic for TimerView.xaml
    /// </summary>
    public partial class TimerView  {
        public TimerView() {
            InitializeComponent();
            SizeChanged += TimerAspectRatioLimits;
        }

        private void TimerAspectRatioLimits(object sender, SizeChangedEventArgs sizeChangedEventArgs) {
            double hToW = TimerText.ActualWidth / TimerText.ActualHeight;
            TimerViewbox.MaxHeight = TimerViewbox.ActualWidth / hToW * 1.5;
            TimerViewbox.MaxWidth = TimerViewbox.ActualHeight * hToW * 1.5;
        }
    }
}