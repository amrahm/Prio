using System.Collections.ObjectModel;

namespace Timer {
    public class TimersService : ITimersService {
        public ObservableCollection<ITimer> Timers { get; } = new ObservableCollection<ITimer>();
    }
}