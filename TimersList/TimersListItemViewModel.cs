using System;
using Infrastructure.SharedResources;
using JetBrains.Annotations;
using Prism.Commands;
using Timer;

namespace TimersList
{
    public class TimersListItemViewModel : NotifyPropertyWithDependencies {
        private readonly ITimer _timer;
        public ITimer Timer {
            get => _timer;
            private init => NotificationBubbler.BubbleSetter(ref _timer, value, (_, _) => this.OnPropertyChanged());
        }

        public enum VisEnableState { Visible, Invisible, Disabled }

        private VisEnableState VisEnable => !Timer.Config.Enabled ?
                VisEnableState.Disabled :
                Timer.Config.Visible ? VisEnableState.Visible : VisEnableState.Invisible;

        [DependsOnProperty(nameof(Timer))]
        public string VisImagePath => VisEnable switch {
            VisEnableState.Visible => @"/Infrastructure;component/SharedResources/images/icons-assets/visible.png",
            VisEnableState.Invisible =>
                    @"/Infrastructure;component/SharedResources/images/icons-assets/invisible.png",
            VisEnableState.Disabled => @"/Infrastructure;component/SharedResources/images/icons-assets/disabled.png",
            _ => throw new ArgumentOutOfRangeException()
        };

        public DelegateCommand OpenTimerSettings { [UsedImplicitly] get; }
        public DelegateCommand ToggleVisState { [UsedImplicitly] get; }
        public DelegateCommand DeleteTimer { [UsedImplicitly] get; }
        public DelegateCommand DuplicateTimer { [UsedImplicitly] get; }
        public TimersListItemViewModel(ITimer timer) {
            Timer = timer;
            OpenTimerSettings = new DelegateCommand(() => Timer.OpenSettings());
            ToggleVisState = new DelegateCommand(() => {
                if(!Timer.Config.Enabled) {
                    Timer.Config.Enabled = true;
                    Timer.SetVisibility(true);
                } else if(Timer.Config.Visible) Timer.ToggleVisibility();
                else Timer.ToggleEnabled();
            });
            DeleteTimer = new DelegateCommand(() => TimersService.Singleton.DeleteTimer(Timer.Config.InstanceID));
            DuplicateTimer = new DelegateCommand(() => {
                ITimer copy = Timer.Duplicate();
                TimersService.Singleton.Timers.Add(copy);
                TimersService.Singleton.SaveSettings();
                copy.ShowTimer();
            });
        }
    }
}