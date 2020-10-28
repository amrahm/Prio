using Prism.Commands;
using Prism.Mvvm;
using Timer;
using Infrastructure.SharedResources;
using Prism.Ioc;

namespace TimersList {
    public class TimersListItemViewModel : BindableBase {
        public ITimer Timer { get; set; }
        public DelegateCommand OpenTimerSettings { get; }

        public TimersListItemViewModel() {
            IContainerProvider container = UnityInstance.GetContainer();
            Timer = container.Resolve<ITimer>();
            OpenTimerSettings = new DelegateCommand(() => {
                Timer.OpenSettings();
            });
        }
    }
}