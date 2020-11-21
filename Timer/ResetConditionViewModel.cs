using Infrastructure.SharedResources;
using static Infrastructure.SharedResources.VirtualDesktopExtensions;

namespace Timer {
    public class ResetConditionViewModel : NotifyPropertyChanged {
        private ResetCondition _model;

        public ResetCondition Model {
            get => _model;
            private set => NotificationBubbler.BubbleSetter(ref _model, value, (o, e) => OnPropertyChanged());
        }

        public ResetConditionViewModel(ResetCondition model) => Model = model;

        public string OffDesktopsConverter {
            get => DesktopSetToString(Model.OffDesktopsSet);
            set => Model.OffDesktopsSet = DesktopStringToSet(value);
        }
    }
}