using System;
using System.Windows;
using Infrastructure.SharedResources;
using MainConfig;
using Prism.Commands;
using Prism.Ioc;

namespace Timer {
    public class TimerViewModel : NotifyPropertyWithDependencies {
        private ITimer _timer;

        public ITimer Timer {
            get => _timer;
            set => NotificationBubbler.BubbleSetter(ref _timer, value, (o, e) => this.OnPropertyChanged());
        }
        private TimerConfig Config => Timer.Config;

        [DependsOnProperty(nameof(Timer))]
        public string TimeLeftVm {
            get {
                TimeSpan timeLeft = Config.TimeLeft;
                if(Config.ShowHours) {
                    return  $"{timeLeft.TotalHours:00}" +
                            $"{(Config.ShowMinutes ? $":{Math.Abs(timeLeft.Minutes):00}" : "")}" +
                            $"{(Config.ShowSeconds ? $":{Math.Abs(timeLeft.Seconds):00}" : "")}";
                }
                if(Config.ShowMinutes) {
                    return  $"{timeLeft.TotalMinutes:00}{(Config.ShowSeconds ? $":{Math.Abs(timeLeft.Seconds):00}" : "")}";
                }
                return Config.ShowSeconds ? $"{timeLeft.TotalSeconds:00}" : timeLeft.ToString();
            }
        }

        [DependsOnProperty(nameof(Timer))]
        public Visibility ShowName => Config.ShowName ? Visibility.Visible : Visibility.Collapsed;

        public DelegateCommand OpenTimerSettings { get; }
        public DelegateCommand OpenMainSettings { get; }
        public DelegateCommand StartStopTimer { get; }
        public DelegateCommand ResetTimer { get; }
        public DelegateCommand ExitProgram { get; }


        public TimerViewModel(ITimer timerTimer) {
            Timer = timerTimer;
            IContainerProvider container = UnityInstance.GetContainer();
            OpenTimerSettings = new DelegateCommand(() => Timer.OpenSettings());
            OpenMainSettings = new DelegateCommand(() => container.Resolve<IMainConfigService>().ShowConfigWindow());
            StartStopTimer = new DelegateCommand(() => {
                if(Timer.IsRunning) Timer.StopTimer();
                else Timer.StartTimer();
            });
            ResetTimer = new DelegateCommand(() => Timer.RequestResetTimer());
            ExitProgram = new DelegateCommand(() => Application.Current.Shutdown());
        }
    }
}