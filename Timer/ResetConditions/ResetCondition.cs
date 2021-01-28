using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Infrastructure.SharedResources;
using Newtonsoft.Json;
using WeakEvent;
using static Infrastructure.SharedResources.UnityInstance;

namespace Timer {
    public enum ResetConditionType { Cooldown, Dependency }

    [Serializable]
    public class ResetCondition : NotifyPropertyChanged {
        public Guid TimerId { get; set; }

        public ResetConditionType Type { get; set; }
        public int SecondsLeft { get; set; }


        public double WaitForMinutes { get; set; }
        public bool OffDesktopsEnabled { get; set; }
        public HashSet<int> OffDesktopsSet { get; set; }

        public Guid DependencyTimerId { get; set; }
        private ITimer _dependencyTimer;
        [JsonIgnore] public ITimer DependencyTimer {
            get => _dependencyTimer ??= TimersService.Singleton?.GetTimer(DependencyTimerId);
            set {
                _dependencyTimer = value;
                if(_dependencyTimer != null) DependencyTimerId = _dependencyTimer.Config.InstanceID;
            }
        }

        public bool MustRunForXEnabled { get; set; }
        public double MustRunForXMinutes { get; set; }
        public bool MustBeFinished { get; set; } = true;
        public bool TimerFinished { get; set; }

        public string UnmetString() {
            string st = "";
            switch(Type) {
                case ResetConditionType.Cooldown:
                    st += SecondsLeft > 60 ? $"Must wait {SecondsLeft / 60} minutes" : $"Must wait {SecondsLeft} seconds";
                    if(OffDesktopsEnabled)
                        st += $" while off of Desktops {VirtualDesktopExtensions.DesktopSetToString(OffDesktopsSet)}";
                    break;
                case ResetConditionType.Dependency:
                    st += $"<italic>{DependencyTimer.Config.Name}</italic> must";
                    if(MustBeFinished) {
                        st += " be finished";
                        if(MustRunForXEnabled) st += " or";
                    }
                    if(MustRunForXEnabled) st += $" run for {SecondsLeft / 60} minutes";
                    break;
            }
            return st;
        }


        private readonly DispatcherTimer _conditionTimer = new()  {Interval = TimeSpan.FromSeconds(1)};

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
        }

        private void OnTimerOnTick(object sender, EventArgs e) {
            switch(Type) {
                case ResetConditionType.Cooldown:
                    if(!OffDesktopsEnabled || !OffDesktopsSet.Contains(VirtualDesktopManager.CurrentDesktop()))
                        SecondsLeft -= 1;
                    break;
                case ResetConditionType.Dependency:
                    if(MustRunForXEnabled && DependencyTimer.Running)
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
                ResetConditionType.Dependency => MustRunForXEnabled && SecondsLeft <= 0 || MustBeFinished && TimerFinished,
                _ => true
            };
        }

        public void Start() {
            if(Type == ResetConditionType.Dependency && MustBeFinished) {
                TimerFinished |= DependencyTimer.Config.TimeLeft.TotalSeconds <= 0;
                DependencyTimer.Finished += OnDependencyTimerOnFinished;
            }
            _conditionTimer.Start();
        }

        private void OnDependencyTimerOnFinished(object o, EventArgs eventArgs) => TimerFinished = true;

        public void StopAndReset() {
            switch(Type) {
                case ResetConditionType.Cooldown:
                    SecondsLeft = (int) (WaitForMinutes * 60);
                    break;
                case ResetConditionType.Dependency:
                    SecondsLeft = (int) (MustRunForXMinutes * 60);
                    TimerFinished = false;
                    if(DependencyTimer != null) DependencyTimer.Finished -= OnDependencyTimerOnFinished;
                    break;
            }
            _conditionTimer.Stop();
        }
    }
}