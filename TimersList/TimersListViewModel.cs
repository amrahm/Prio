using System.Collections.ObjectModel;
using Prism.Commands;
using Infrastructure.SharedResources;
using Prism.Ioc;
using Prism.Mvvm;
using Timer;

namespace TimersList {
    public class TimersListViewModel : BindableBase {
        public ObservableCollection<TimersListItemView> Timers { get; } = new ObservableCollection<TimersListItemView>();

        public DelegateCommand AddTimerCommand { get; set; }

        public TimersListViewModel() {
            IContainerProvider container = UnityInstance.GetContainer();
            var timersService = container.Resolve<ITimersService>();

            // Load existing timers
            foreach(ITimer timer in timersService.Timers) Timers.Add(new TimersListItemView(timer));

            // Update if a timer is added elsewhere
            timersService.Timers.CollectionChanged += (sender, e) => {
                foreach(ITimer timer in e.NewItems) Timers.Add(new TimersListItemView(timer));
            };

            // Add new timer on button press
            AddTimerCommand = new DelegateCommand(() => {
                ITimer timer = container.Resolve<ITimer>();
                Timers.Add(new TimersListItemView(timer));
                timer.OpenSettings(); //TODO they can cancel and leave a null timer name
            });
        }
    }
}