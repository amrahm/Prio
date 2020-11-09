using System;
using System.Windows;
using Infrastructure.SharedResources;
using JetBrains.Annotations;
using MainConfig;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;

namespace Timer {
    public class TimerViewModel : NotifyPropertyWithDependencies, IDialogAware {
        public string Title { get; set; } = "Timer";
        public event Action<IDialogResult> RequestClose;

        private ITimer _timer;

        public ITimer Timer {
            get => _timer;
            set => NotificationBubbler.BubbleSetter(ref _timer, value, (o, e) => this.OnPropertyChanged());
        }

        [DependsOnProperty(nameof(Timer))]
        [UsedImplicitly] public string TimeLeftVm {
            get {
                TimeSpan timeLeft = Timer.Config.TimeLeft;
                if(Timer.Config.ShowHours) {
                    string tl = $"{(int) timeLeft.TotalHours:00}";
                    if(Timer.Config.ShowMinutes) tl += $":{timeLeft.Minutes:00}";
                    if(Timer.Config.ShowSeconds) tl += $":{timeLeft.Seconds:00}";
                    return  tl;
                }
                if(Timer.Config.ShowMinutes) {
                    string tl = $"{(int) timeLeft.TotalMinutes:00}";
                    if(Timer.Config.ShowSeconds) tl += $":{timeLeft.Seconds:00}";
                    return  tl;
                }
                return Timer.Config.ShowSeconds ? $"{(int) timeLeft.TotalSeconds:00}" : timeLeft.ToString();
            }
        }

        [DependsOnProperty(nameof(Timer))]
        [UsedImplicitly] public Visibility ShowName => Timer.Config.ShowName ? Visibility.Visible : Visibility.Collapsed;

        public DelegateCommand OpenTimerSettings { [UsedImplicitly] get; }
        public DelegateCommand OpenMainSettings { [UsedImplicitly] get; }
        public DelegateCommand StartStopTimer { [UsedImplicitly] get; }
        public DelegateCommand ExitProgram { [UsedImplicitly] get; }


        private TimerViewModel() {
            IContainerProvider container = UnityInstance.GetContainer();
            OpenTimerSettings = new DelegateCommand(() => Timer.OpenSettings());
            OpenMainSettings = new DelegateCommand(() => container.Resolve<IMainConfigService>().ShowConfigWindow());
            StartStopTimer = new DelegateCommand(() => {
                if(Timer.IsRunning) Timer.StopTimer();
                else Timer.StartTimer();
            });
            ExitProgram = new DelegateCommand(() => Application.Current.Shutdown());
        }

        public TimerViewModel(ITimer timerTimer) : this() => Timer = timerTimer;

        public void OnDialogOpened(IDialogParameters parameters) {
            Timer = parameters.GetValue<ITimer>(nameof(ITimer));
            Title = Timer.Config.Name;
        }

        public bool CanCloseDialog() => true;
        public void OnDialogClosed() => Timer.SaveSettings();
    }
}