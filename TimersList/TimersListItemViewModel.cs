using JetBrains.Annotations;
using Prism.Commands;
using Prism.Mvvm;
using Timer;

namespace TimersList {
    public class TimersListItemViewModel : BindableBase{
        public ITimer Timer { get; set; }
        public DelegateCommand OpenTimerSettings { [UsedImplicitly] get; }

        public TimersListItemViewModel() {
            OpenTimerSettings = new DelegateCommand(() => Timer.OpenSettings());
        }
    }
}