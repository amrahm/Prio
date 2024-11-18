using System.Collections.ObjectModel;
using System.Linq;
using Prism.Commands;
using Prism.Ioc;
using Timer;
using Infrastructure.SharedResources;
using Prism.Dialogs;
using static Infrastructure.SharedResources.UnityInstance;

namespace TimersList {
    public class TimersListViewModel : NotifyPropertyChanged {
        public ObservableCollection<TimersListItemView> Timers { get; } = new();

        public DelegateCommand AddTimerCommand { get; }

        public TimersListViewModel() {
            // Load existing timers
            foreach(ITimer timer in TimersService.Singleton.Timers)
                Timers.Add(new TimersListItemView(new TimersListItemViewModel(timer)));

            // Update if a timer is added/removed
            TimersService.Singleton.Timers.CollectionChanged += (_, e) => {
                if(e.NewItems != null)
                    foreach(ITimer timer in e.NewItems)
                        Timers.Add(new TimersListItemView(new TimersListItemViewModel(timer)));
                if(e.OldItems != null)
                    foreach(ITimer timer in e.OldItems)
                        Timers.Remove(Timers.Single(tl => tl.ViewModel.Timer.Config.InstanceID == timer.Config.InstanceID));
            };

            // Add new timer on button press
            AddTimerCommand = new DelegateCommand(() => {
                ITimer timer = Container.Resolve<ITimer>();
                if(timer.OpenSettings().Result == ButtonResult.OK) {
                    TimersService.Singleton.Timers.Add(timer);
                    TimersService.Singleton.SaveSettings();
                    timer.ShowTimer();
                }
            });
        }
    }
}
