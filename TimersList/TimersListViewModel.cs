using System.Collections.ObjectModel;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using static Infrastructure.SharedResources.UnityInstance;

namespace TimersList {
    public class TimersListViewModel : BindableBase {
        public ObservableCollection<TimersListItemView> Timers { get; } = new ObservableCollection<TimersListItemView>();
        //private readonly TimersListModel _model;

        public DelegateCommand AddTimerCommand { get; set; }

        public TimersListViewModel() {
            //_model = LoadSettings<TimersListModel>(TIMERS_LIST) ?? new TimersListModel(this);
            var  container = GetContainer();
            AddTimerCommand = new DelegateCommand(() => {
                TimersListItemView view = container.Resolve<TimersListItemView>();
                Timers.Add(view);
            });
        }
    }
}