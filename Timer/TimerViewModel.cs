using System;
using System.Windows;
using System.Windows.Media;
using Infrastructure.SharedResources;
using MainConfig;
using Prism.Commands;
using Prism.Ioc;

namespace Timer {
    public class TimerViewModel : NotifyPropertyWithDependencies {
        private readonly ITimer _timer;

        public ITimer Timer {
            get => _timer;
            init => NotificationBubbler.BubbleSetter(ref _timer, value, (_, _) => this.OnPropertyChanged());
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

        [DependsOnProperty(nameof(Timer))] public Brush BackgroundColor => Config.BackgroundColor;
        [DependsOnProperty(nameof(Timer))] public Brush TextColor => Config.TextColor;
        [DependsOnProperty(nameof(Timer))] public Brush NameBackgroundColor => Config.NameBackgroundColor;
        [DependsOnProperty(nameof(Timer))] public Brush NameTextColor => Config.NameTextColor;
        [DependsOnProperty(nameof(Timer))] public Brush DividerColor => Config.DividerColor;

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