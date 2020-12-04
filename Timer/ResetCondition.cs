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
        private ITimer _timer;
        [JsonIgnore] public ITimer Timer {
            get => _timer ??= TimersService.Singleton.GetTimer(TimerId);
            set {
                _timer = value;
                if(_timer != null) TimerId = _timer.Config.InstanceID;
            }
        }
        public ResetConditionType Type { get; set; }

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

        public bool MustRunForXEnabled { get; set; }
        public double MustRunForXMinutes { get; set; }
        public bool MustBeFinished { get; set; }
        private bool _timerFinished;

        private readonly DispatcherTimer _conditionTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
        private int _secondsLeft;
        private readonly IVirtualDesktopManager _virtualDesktopManager;

        private readonly WeakEventSource<EventArgs> _deleteRequested = new WeakEventSource<EventArgs>();
        public event EventHandler<EventArgs> DeleteRequested {
            add => _deleteRequested.Subscribe(value);
            remove => _deleteRequested.Unsubscribe(value);
        }

        private readonly WeakEventSource<EventArgs> _satisfied = new WeakEventSource<EventArgs>();
        public event EventHandler<EventArgs> Satisfied {
            add => _satisfied.Subscribe(value);
            remove => _satisfied.Unsubscribe(value);
        }

        public virtual void DeleteMe() {
            _deleteRequested.Raise(this, EventArgs.Empty);
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
                    if(!OffDesktopsEnabled || OffDesktopsSet.Contains(_virtualDesktopManager.CurrentDesktop()))
                        _secondsLeft -= 1;
                    break;
                case ResetConditionType.Dependency:
                    if(MustRunForXEnabled && DependencyTimer.IsRunning)
                        _secondsLeft -= 1;
                    break;
            }

            if(IsSatisfied()) {
                _satisfied.Raise(this, EventArgs.Empty);
                _conditionTimer.Stop();
            }
        }

        public bool IsSatisfied() {
            return Type switch {
                ResetConditionType.Cooldown => _secondsLeft <= 0,
                ResetConditionType.Dependency => (!MustRunForXEnabled || _secondsLeft <= 0) &&
                                                 (!MustBeFinished || _timerFinished),
                _ => true
            };
        }

        public void Start() {
            switch(Type) {
                case ResetConditionType.Cooldown:
                    _secondsLeft = (int) (WaitForMinutes*60);
                    break;
                case ResetConditionType.Dependency:
                    _secondsLeft = (int) (MustRunForXMinutes*60);
                    if(MustBeFinished) {
                        _timerFinished = DependencyTimer.Config.TimeLeft.TotalSeconds <= 0;
                        DependencyTimer.Finished += (sender, e) => _timerFinished = true;
                    }
                    break;
            }
            _conditionTimer.Start();
        }

        public void Stop() => _conditionTimer.Stop();
    }
}