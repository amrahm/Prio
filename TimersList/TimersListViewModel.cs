using System.Collections.ObjectModel;
using Prism.Commands;
using Prism.Mvvm;
using Infrastructure.SharedResources;
using Prism.Ioc;

namespace TimersList {
    public class TimersListViewModel : BindableBase {
        public ObservableCollection<TimersListItemView> Timers { get; } = new ObservableCollection<TimersListItemView>();

        public DelegateCommand AddTimerCommand { get; set; }

        public TimersListViewModel() {
            IContainerProvider container = UnityInstance.GetContainer();
            AddTimerCommand = new DelegateCommand(() => {
                TimersListItemView view = container.Resolve<TimersListItemView>();
                Timers.Add(view);
            });
        }
    }
}