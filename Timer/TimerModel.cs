using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Infrastructure.SharedResources;
using Prio.GlobalServices;
using Prism.Ioc;
using Prism.Services.Dialogs;
using WeakEvent;

namespace Timer {
    public class TimerModel : NotifyPropertyWithDependencies, ITimer {
        private TimerConfig _config;

        public TimerConfig Config {
            get => _config;
            set {
                void ResetConditionsOnSatisfied(object sender, EventArgs e) {
                    if(Config.AutoResetOnConditions) {
                        ResetTimer();
                        StartStopForDesktopsActive(_vdm.CurrentDesktop());
                    }
                }

                if(_config != null) _config.ResetConditions.Satisfied -= ResetConditionsOnSatisfied;
                NotificationBubbler.BubbleSetter(ref _config, value, (_, _) => this.OnPropertyChanged());
                if(_config != null) {
                    _config.ResetConditions.Satisfied += ResetConditionsOnSatisfied;
                    SetupTimerActions();
                }
            }
        }

        public Window TimerWindow { get; private set; }
        public Brush TempBackgroundBrush { get; set; }
        public Brush TempTextBrush { get; set; }

        private readonly WeakEventSource<EventArgs> _finished = new();
        public event EventHandler<EventArgs> Finished {
            add => _finished.Subscribe(value);
            remove => _finished.Unsubscribe(value);
        }

        private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
        private readonly IDialogService _dialogService;
        private readonly DispatcherTimer _timer = new() {Interval = OneSecond};
        private bool _hidden;
        private bool _finishedSet;
        private readonly IVirtualDesktopManager _vdm;

        public bool IsRunning => _timer.IsEnabled;

        public TimerModel(TimerConfig config) {
            Config = config;
            IContainerProvider container = UnityInstance.GetContainer();
            _dialogService = container.Resolve<IDialogService>();

            SetupTimerActions();
            _timer.Tick += OnTimerOnTick;

            RegisterShortcuts();

            _vdm = container.Resolve<IVirtualDesktopManager>();
            _vdm.DesktopChanged += (_, e) => HandleDesktopChanged(e.NewDesktop);
        }

        public void CheckStart() {
            if(Config.TimeLeft.TotalSeconds <= 0.01) TimerFinishedRaise();
        }

        private void OnTimerOnTick(object o, EventArgs e) {
            Config.TimeLeft -= OneSecond;
            CheckTimerActions();
        }

        private void TimerFinishedRaise() {
            _finishedSet = true;
            _finished.Raise(this, EventArgs.Empty);
            if(!Config.OverflowEnabled) StopTimer();
            Config.ResetConditions.StartConditions();
        }

        #region TimerActions

        private class TimerAction : IComparable<TimerAction> {
            public TimeSpan TriggerTime { get; }
            public Action Action { get; }

            public TimerAction(TimeSpan triggerTime, Action action) {
                TriggerTime = triggerTime;
                Action = action;
            }

            public int CompareTo(TimerAction other) => -TriggerTime.CompareTo(other.TriggerTime);
        }

        private readonly List<TimerAction> _timerActions = new();
        private int _timerActionsPointer;

        private void SetupTimerActions() {
            _timerActions.Clear();
            _timerActions.Add(new TimerAction(TimeSpan.Zero, TimerFinishedRaise));
            _timerActions.Add(new TimerAction(TimeSpan.Zero, Config.ZeroOverflowAction.DoAction));
            foreach(var x in Config.OverflowActions)
                _timerActions.Add(new TimerAction(TimeSpan.FromMinutes(-x.AfterMinutes), x.DoAction));
            _timerActions.Sort();
            _timerActionsPointer = 0;
            while(_timerActionsPointer < _timerActions.Count &&
                  Config.TimeLeft <= _timerActions[_timerActionsPointer].TriggerTime) _timerActionsPointer++;
        }

        private void CheckTimerActions() {
            while(_timerActionsPointer < _timerActions.Count &&
                  Config.TimeLeft <= _timerActions[_timerActionsPointer].TriggerTime) {
                _timerActions[_timerActionsPointer].Action(); //TODO timer hangs
                _timerActionsPointer++;
            }
        }

        #endregion

        #region VirtualDesktops

        private void HandleDesktopChanged(int newDesktop) {
            TimerWindow?.Dispatcher.Invoke(() => {
                Config.DesktopsVisible ??= new HashSet<int>();
                if((Config.DesktopsVisible.Contains(-1) || Config.DesktopsVisible.Contains(newDesktop)) && !_hidden &&
                   TimersService.Singleton.currVisState != VisibilityState.Hidden) {
                    TimerWindow.Visibility = Visibility.Visible;
                    _vdm.MoveToDesktop(TimerWindow, newDesktop);
                } else if(Config.DesktopsVisible.Count > 0)  {
                    TimerWindow.Visibility = Visibility.Hidden;
                }
            });

            StartStopForDesktopsActive(newDesktop);
        }

        private void StartStopForDesktopsActive(int newDesktop) {
            Config.DesktopsActive ??= new HashSet<int>();
            if(Config.DesktopsActive.Contains(-1) || Config.DesktopsActive.Contains(newDesktop)) StartTimer();
            else if(Config.DesktopsVisible.Count > 0) StopTimer();
        }

        #endregion

        #region ShowHide

        public void ShowTimer() {
            if(TimerWindow != null) {
                TimerWindow.Activate();
                return;
            }

            TimerWindow = new TimerWindow {Content = new TimerView(new TimerViewModel(this)), Title = Config.Name};
            TimerWindow.Show();
            TimerWindow.Closed += (_,  _) => {
                SaveSettings();
                TimerWindow = null;
            };

            TimersService.Singleton.ApplyVisState();
            HandleDesktopChanged(_vdm.CurrentDesktop());
        }

        private void ShowHideTimer() {
            if(TimerWindow == null) return;
            if(_hidden) {
                TimerWindow.Visibility = Visibility.Visible;
                TimerWindow.Activate();
            } else TimerWindow.Visibility = Visibility.Hidden;
            _hidden = !_hidden;
        }

        #endregion

        #region Reset

        private void ResetTimer() {
            Config.TimeLeft = Config.Duration;
            _finishedSet = false;
            _timerActionsPointer = 0;
            Config.ResetConditions.StopAndResetAllConditions();
        }

        public void RequestResetTimer() {
            if(_finishedSet && Config.ResetConditions.IsSat() || !_finishedSet && Config.AllowResetWhileRunning) {
                ResetTimer();
                return;
            }

            string unmetStrings = _finishedSet ? Config.ResetConditions.UnmetStrings() : "The timer has not finished yet.";

            string  message = $"Not all reset conditions are met:\n\n<Bold>{unmetStrings}</Bold>";
            if(Config.AllowResetOverride) {
                IDialogResult r = _dialogService.ShowNotification(message + "\n\nDo you want to override?",
                                                                  $"Resetting {Config.Name}", hasCancel: true,
                                                                  customOk: "YES", customCancel: "NO").Result;
                if(r.Result == ButtonResult.OK) ResetTimer();
            } else {
                _dialogService.ShowNotification(message, $"Unable to Reset {Config.Name}");
            }
        }

        #endregion

        public void StartTimer() {
            if(Config.OverflowEnabled || Config.TimeLeft.TotalSeconds > 0) {
                _timer.Start();
                TimersService.Singleton.isStopAll = false;
            }
        }

        public void StopTimer() => _timer.Stop();

        #region Settings

        public async Task<ButtonResult> OpenSettings() {
            IDialogResult r = await _dialogService.ShowDialogAsync(nameof(TimerSettingsView),
                                                                   new DialogParameters {{nameof(ITimer), this}});
            return r.Result;
        }

        public void SaveSettings() {
            TimersService.Singleton.SaveSettings();
            RegisterShortcuts();
        }

        #endregion

        #region Shortcuts

        private enum TimerHotkeyState { ShouldStart, ShouldStop }

        private void RegisterShortcuts() {
            IContainerProvider container = UnityInstance.GetContainer();
            var hotkeyManager = container.Resolve<IPrioHotkeyManager>();
            hotkeyManager.RegisterHotkey(Config.InstanceID, nameof(Config.ResetShortcut), Config.ResetShortcut,
                                         RequestResetTimer, CompatibilityType.Reset);

            int NextTimerState(int r) => (int) (IsRunning ? TimerHotkeyState.ShouldStop : TimerHotkeyState.ShouldStart);
            hotkeyManager.RegisterHotkey(Config.InstanceID, nameof(Config.StartShortcut), Config.StartShortcut, StartTimer,
                                         CompatibilityType.StartStop, (int) TimerHotkeyState.ShouldStart, NextTimerState);
            hotkeyManager.RegisterHotkey(Config.InstanceID, nameof(Config.StopShortcut), Config.StopShortcut, StopTimer,
                                         CompatibilityType.StartStop, (int) TimerHotkeyState.ShouldStop, NextTimerState);

            hotkeyManager.RegisterHotkey(Config.InstanceID, nameof(Config.ShowHideShortcut), Config.ShowHideShortcut,
                                         ShowHideTimer, CompatibilityType.Visibility);
        }

        #endregion

        public override bool Equals(object obj) => obj is TimerModel other && other.Config.InstanceID == Config.InstanceID;
        public override int GetHashCode() => Config.InstanceID.GetHashCode();
        public override string ToString() => Config.Name;
    }
}