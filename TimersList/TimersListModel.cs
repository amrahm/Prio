using System.Collections.Generic;
using Timer;

namespace TimersList {
    class TimersListModel {
        private IList<ITimer> _timers;
        private TimersListViewModel _vm;

        public TimersListModel(TimersListViewModel vm) {
            //TODO load timers
            _vm = vm;
            //_timers = new List<ITimer> {new TimerViewModel()};
            //vm.Timers = _timers;
        }
    }
}