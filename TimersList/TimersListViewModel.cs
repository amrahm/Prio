using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using static Infrastructure.Constants.ModuleNames;
using static Infrastructure.SharedResources.Settings;

namespace TimersList {
    class TimersListViewModel : BindableBase {
        private readonly TimersListModel _model;

        public TimersListViewModel() {
            _model = LoadSettings<TimersListModel>(TIMERS_LIST) ?? new TimersListModel();
        }
    }
}