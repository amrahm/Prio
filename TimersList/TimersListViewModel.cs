using System.Collections.ObjectModel;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Timer;

namespace TimersList {
    public class TimersListViewModel : BindableBase {
        public ObservableCollection<TimersListItemView> Timers { get; } = new ObservableCollection<TimersListItemView>();

        public DelegateCommand AddTimerCommand { get; set; }

        public TimersListViewModel() {
            IContainerProvider container = Infrastructure.SharedResources.UnityInstance.GetContainer();

            // Load existing timers
            foreach(ITimer timer in TimersService.Singleton.Timers) Timers.Add(new TimersListItemView(timer));

            // Update if a timer is added elsewhere
            TimersService.Singleton.Timers.CollectionChanged += (sender, e) => {
                foreach(ITimer timer in e.NewItems) Timers.Add(new TimersListItemView(timer));
            };

            // Add new timer on button press
            AddTimerCommand = new DelegateCommand(() => {
                ITimer timer = container.Resolve<ITimer>();
                TimersService.Singleton.Timers.Add(timer);
                //Timers.Add(new TimersListItemView(timer)); //TODO put indicator for timer being hidden, but not disabled
                //timer.OpenSettings(); //TODO they can cancel and leave a null timer name
            });
        }
    }
}