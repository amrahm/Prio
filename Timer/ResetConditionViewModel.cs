using System.Collections.Generic;
using System.Linq;
using Infrastructure.SharedResources;
using Prism.Commands;
using static Infrastructure.SharedResources.VirtualDesktopExtensions;

namespace Timer {
    public class ResetConditionViewModel : NotifyPropertyChanged {
        private ResetCondition _model;
        private ITimer _selectedTimer;

        public ResetCondition Model {
            get => _model;
            private set => NotificationBubbler.BubbleSetter(ref _model, value, (o, e) => OnPropertyChanged());
        }

        public IEnumerable<ITimer> Timers =>
                TimersService.Singleton.Timers.Where(x => !x.Config.InstanceID.Equals(Model.TimerId));


        public ITimer SelectedTimer {
            get => _selectedTimer ??= Timers.FirstOrDefault(x => x.Config.InstanceID == Model.DependencyTimerId);
            set {
                _selectedTimer = value;
                if(_selectedTimer != null) Model.DependencyTimerId = _selectedTimer.Config.InstanceID;
            }
        }

        public DelegateCommand DeleteCommand { get; }

        public ResetConditionViewModel(ResetCondition model) {
            Model = model;
            DeleteCommand = new DelegateCommand(() => Model.DeleteMe());
        }

        public string OffDesktopsConverter {
            get => DesktopSetToString(Model.OffDesktopsSet);
            set => Model.OffDesktopsSet = DesktopStringToSet(value);
        }
    }
}