using Infrastructure.SharedResources;
using JetBrains.Annotations;
using Prism.Commands;
using Timer;

namespace TimersList {
    public class TimersListItemViewModel : NotifyPropertyChanged {
        public ITimer Timer { get; }
        public DelegateCommand OpenTimerSettings { [UsedImplicitly] get; }

        public TimersListItemViewModel(ITimer timer) {
            Timer = timer;
            OpenTimerSettings = new DelegateCommand(() => Timer.OpenSettings());
        }
    }
}