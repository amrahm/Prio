using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Timer;

namespace TimersList {
    public class TimersListViewModel : BindableBase {
        public ObservableCollection<TimersListItemView> Timers { get; } = new ObservableCollection<TimersListItemView>();

        public DelegateCommand AddTimerCommand { get; set; }

        public TimersListViewModel() {
            IContainerProvider container = Infrastructure.SharedResources.UnityInstance.GetContainer();

            // Load existing timers
            foreach(ITimer timer in TimersService.Singleton.Timers)
                Timers.Add(new TimersListItemView(new TimersListItemViewModel(timer)));

            // Update if a timer is added
            TimersService.Singleton.Timers.CollectionChanged += (sender, e) => {
                foreach(ITimer timer in e.NewItems) Timers.Add(new TimersListItemView(new TimersListItemViewModel(timer)));
            };

            // Add new timer on button press
            AddTimerCommand = new DelegateCommand(async () => {
                ITimer timer = container.Resolve<ITimer>();
                Task<ButtonResult> resTask = timer.OpenSettings();
                ButtonResult res = await resTask;
                if(res == ButtonResult.OK) TimersService.Singleton.Timers.Add(timer);
                TimersService.Singleton.SaveSettings();
            });
        }
    }
}