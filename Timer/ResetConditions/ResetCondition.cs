using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Infrastructure.SharedResources;
using Newtonsoft.Json;
using Prio.GlobalServices;
using Prism.Ioc;
using WeakEvent;

namespace Timer {
    public enum ResetConditionType { Cooldown, Dependency }

    [Serializable]
    public class ResetCondition : NotifyPropertyChanged {
        public Guid TimerId { get; set; }

        public ResetConditionType Type { get; set; }
        public int SecondsLeft { get; private set; }


        public double WaitForMinutes { get; set; }
        public bool OffDesktopsEnabled { get; set; }
        public HashSet<int> OffDesktopsSet { get; set; }

        public Guid DependencyTimerId { get; set; }
        private ITimer _dependencyTimer;
        [JsonIgnore] public ITimer DependencyTimer {
            get => _dependencyTimer ??= TimersService.Singleton.GetTimer(DependencyTimerId);
            set {
                _dependencyTimer = value;
                if(_dependencyTimer != null) DependencyTimerId = _dependencyTimer.Config.InstanceID;
            }
        }

        public bool MustRunForXEnabled { get; set; } //TODO validation to ensure this or MustBeFinished is true, but not both
        public double MustRunForXMinutes { get; set; }
        public bool MustBeFinished { get; set; }
        public bool TimerFinished { get; private set; }

        public string UnmetString() {
            string st = "";
            switch(Type) {
                case ResetConditionType.Cooldown:
                    st += SecondsLeft > 60 ? $"Must wait {SecondsLeft / 60} minutes" : $"Must wait {SecondsLeft} seconds";
                    if(OffDesktopsEnabled)
                        st += $" while off of Desktops {VirtualDesktopExtensions.DesktopSetToString(OffDesktopsSet)}";
                    break;
                case ResetConditionType.Dependency:
                    st += $"{DependencyTimer.Config.Name} must";
                    if(MustBeFinished) st += " be finished";
                    else if(MustRunForXEnabled) st += $" run for {SecondsLeft / 60} minutes";
                    break;
            }
            return st + ".";
        }


        private readonly DispatcherTimer _conditionTimer = new()  {Interval = TimeSpan.FromSeconds(1)};
        private readonly IVirtualDesktopManager _virtualDesktopManager;

        private readonly WeakEventSource<EventArgs> _deleteRequested = new();
        public event EventHandler<EventArgs> DeleteRequested {
            add => _deleteRequested.Subscribe(value);
            remove => _deleteRequested.Unsubscribe(value);
        }
        public void DeleteMe() => _deleteRequested.Raise(this, EventArgs.Empty);

        private readonly WeakEventSource<EventArgs> _satisfied = new();
        public event EventHandler<EventArgs> Satisfied {
            add => _satisfied.Subscribe(value);
            remove => _satisfied.Unsubscribe(value);
        }

        [JsonConstructor]
        private ResetCondition() : this(null) { } //Needed for deserializing

        public ResetCondition(ITimer timer) {
            if(timer != null) TimerId = timer.Config.InstanceID;
            _conditionTimer.Tick += OnTimerOnTick;
            _virtualDesktopManager = UnityInstance.GetContainer().Resolve<IVirtualDesktopManager>();
        }

        private void OnTimerOnTick(object sender, EventArgs e) {
            switch(Type) {
                case ResetConditionType.Cooldown:
                    if(!OffDesktopsEnabled || !OffDesktopsSet.Contains(_virtualDesktopManager.CurrentDesktop()))
                        SecondsLeft -= 1;
                    break;
                case ResetConditionType.Dependency:
                    if(MustRunForXEnabled && DependencyTimer.IsRunning)
                        SecondsLeft -= 1;
                    break;
            }

            if(IsSatisfied()) {
                _satisfied.Raise(this, EventArgs.Empty);
                _conditionTimer.Stop();
            }
        }

        public bool IsSatisfied() {
            return Type switch {
                ResetConditionType.Cooldown => SecondsLeft <= 0,
                ResetConditionType.Dependency => (!MustRunForXEnabled || SecondsLeft <= 0) &&
                                                 (!MustBeFinished || TimerFinished),
                _ => true
            };
        }

        public void Start() {
            switch(Type) {
                case ResetConditionType.Cooldown:
                    SecondsLeft = (int) (WaitForMinutes * 60);
                    break;
                case ResetConditionType.Dependency:
                    SecondsLeft = (int) (MustRunForXMinutes * 60);
                    if(MustBeFinished) {
                        TimerFinished = DependencyTimer.Config.TimeLeft.TotalSeconds <= 0;
                        DependencyTimer.Finished += (_, _) => TimerFinished = true;
                    }
                    break;
            }
            _conditionTimer.Start();
        }

        public void Stop() => _conditionTimer.Stop();
    }
}