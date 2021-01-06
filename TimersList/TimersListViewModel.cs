using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Timer;

namespace TimersList {
    public class TimersListViewModel : BindableBase {
        public ObservableCollection<TimersListItemView> Timers { get; } = new();

        public DelegateCommand AddTimerCommand { get; }

        public TimersListViewModel() {
            IContainerProvider container = Infrastructure.SharedResources.UnityInstance.GetContainer();

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
            AddTimerCommand = new DelegateCommand(async () => {
                ITimer timer = container.Resolve<ITimer>();
                Task<ButtonResult> resTask = timer.OpenSettings();
                ButtonResult res = await resTask;
                if(res == ButtonResult.OK) {
                    TimersService.Singleton.Timers.Add(timer);
                    TimersService.Singleton.SaveSettings();
                    timer.ShowTimer();
                }
            });
        }
    }
}