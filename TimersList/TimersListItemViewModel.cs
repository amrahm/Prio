using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Timer;
using static Infrastructure.SharedResources.UnityInstance;

namespace TimersList {
    public class TimersListItemViewModel : BindableBase {
        public ITimer Timer { get; set; }
        public DelegateCommand OpenTimerSettings { get; }

        public TimersListItemViewModel() {
            var  container = GetContainer();
            Timer = container.Resolve<ITimer>();
            OpenTimerSettings = new DelegateCommand(() => {
                Timer.OpenSettings();
            });
        }
    }
}