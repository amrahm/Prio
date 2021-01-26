using System;
using Infrastructure.SharedResources;
using JetBrains.Annotations;
using Prism.Commands;
using Timer;

namespace TimersList {
    public class TimersListItemViewModel : NotifyPropertyWithDependencies {
        private readonly ITimer _timer;
        public ITimer Timer {
            get => _timer;
            private init => NotificationBubbler.BubbleSetter(ref _timer, value, (_, _) => this.OnPropertyChanged());
        }

        public enum VisEnableState { Visible, Invisible, Disabled }

        private VisEnableState VisEnable => Timer.Config.Disabled ?
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
        public TimersListItemViewModel(ITimer timer) {
            Timer = timer;
            OpenTimerSettings = new DelegateCommand(() => Timer.OpenSettings());
            ToggleVisState = new DelegateCommand(() => {
                if(Timer.Config.Disabled) {
                    Timer.Config.Disabled = false;
                    Timer.SetVisibility(true);
                } else if(Timer.Config.Visible) Timer.ToggleVisibility();
                else Timer.ToggleEnabled();
            });
            DeleteTimer = new DelegateCommand(() => {
                //TODO 
            });
        }
    }
}