using System.Windows;

namespace TimersList
{
    /// <summary>
    /// Interaction logic for TimersListTimerView.xaml
    /// </summary>
    public partial class TimersListTimerView  {
        public TimersListTimerView()
        {
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
