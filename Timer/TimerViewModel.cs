using System;
using System.Windows;
using System.Windows.Media;
using Infrastructure.SharedResources;
using MainConfig;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using static Infrastructure.SharedResources.UnityInstance;

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

        [DependsOnProperty(nameof(Timer))]
        public Brush BackgroundColor => Timer.TempBackgroundBrush ?? Config.BackgroundColor;
        [DependsOnProperty(nameof(Timer))]
        public Brush TextColor => Timer.TempTextBrush ?? Config.TextColor;

        public DelegateCommand OpenTimerSettings { get; }
        public DelegateCommand OpenMainSettings { get; }
        public DelegateCommand StartStopTimer { get; }
        public DelegateCommand ResetTimer { get; }
        public DelegateCommand<object> AddMinutes { get; }
        public DelegateCommand SetTime { get; }
        public DelegateCommand ExitProgram { get; }


        public TimerViewModel(ITimer timerTimer) {
            Timer = timerTimer;
            OpenTimerSettings = new DelegateCommand(() => Timer.OpenSettings());
            OpenMainSettings = new DelegateCommand(() => Container.Resolve<IMainConfigService>().ShowConfigWindow());
            StartStopTimer = new DelegateCommand(() => {
                if(Timer.IsRunning) Timer.StopTimer();
                else Timer.StartTimer();
            });
            ResetTimer = new DelegateCommand(() => Timer.RequestResetTimer());
            AddMinutes = new DelegateCommand<object>(m => Timer.AddMinutes(int.Parse((string) m)));
            SetTime = new DelegateCommand(async () => {
                bool wasRunning = Timer.IsRunning;
                Timer.StopTimer();
                var r = await Dialogs.ShowDialogAsync(nameof(ChangeTimeView),
                                                      new DialogParameters {
                                                          {nameof(ChangeTimeViewModel.Title), Config.Name},
                                                          {nameof(ChangeTimeViewModel.Duration), Config.TimeLeft}
                                                      });
                if(r.Result == ButtonResult.OK)
                    Timer.SetTime(r.Parameters.GetValue<TimeSpan>(nameof(ChangeTimeViewModel.Duration)));
                if(wasRunning) Timer.StartTimer();
            });
            ExitProgram = new DelegateCommand(() => Application.Current.Shutdown());
        }
    }
}