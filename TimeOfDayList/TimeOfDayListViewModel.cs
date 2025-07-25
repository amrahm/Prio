﻿using System.Collections.ObjectModel;
using System.Linq;
using Prism.Commands;
using Prism.Ioc;
using Timer;
using Infrastructure.SharedResources;
using static Infrastructure.SharedResources.UnityInstance;

namespace TimeOfDayList {
    public class TimeOfDayListViewModel : NotifyPropertyChanged {
        public ObservableCollection<TimeOfDayListItemView> Timers { get; } = new();

        public DelegateCommand AddTimerCommand { get; }

        public TimeOfDayListViewModel() {
            // Load existing timers
            foreach(ITimer timer in TimersService.Singleton.Timers)
                Timers.Add(new TimeOfDayListItemView(new TimeOfDayListItemViewModel(timer)));

            // Update if a timer is added/removed
            TimersService.Singleton.Timers.CollectionChanged += (_, e) => {
                if(e.NewItems != null)
                    foreach(ITimer timer in e.NewItems)
                        Timers.Add(new TimeOfDayListItemView(new TimeOfDayListItemViewModel(timer)));
                if(e.OldItems != null)
                    foreach(ITimer timer in e.OldItems)
                        Timers.Remove(Timers.Single(tl => tl.ViewModel.Timer.Config.InstanceID == timer.Config.InstanceID));
            };

            // Open settinfgs for a new timer
            AddTimerCommand = new DelegateCommand(() => {
                ITimer timer = Container.Resolve<ITimer>();
                timer.OpenSettings();
            });
        }
    }
}
