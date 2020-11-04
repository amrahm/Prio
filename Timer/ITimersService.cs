using System.Collections.ObjectModel;

namespace Timer {
    public interface ITimersService {
        ObservableCollection<ITimer> Timers { get; } //TODO have to handle adding new timers to this
    }
}