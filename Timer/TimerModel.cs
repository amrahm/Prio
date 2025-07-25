﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Infrastructure.SharedResources;
using Microsoft.Win32;
using Prio.GlobalServices;
using Prism.Dialogs;
using WeakEvent;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using static Infrastructure.SharedResources.UnityInstance;

namespace Timer {
    public class TimerModel : NotifyPropertyWithDependencies, ITimer {
        private TimerConfig _config;

        public TimerConfig Config {
            get => _config;
            set {
                if(_config != null) {
                    _config.ResetConditions.Satisfied -= ResetConditionsOnSatisfied;
                    SystemEvents.SessionSwitch -= SysEventsCheck;
                    UnregisterShortcuts();
                }

                NotificationBubbler.BubbleSetter(ref _config, value, (_, _) => this.OnPropertyChanged());

                if(_config is {Enabled: true}) {
                    RegisterShortcuts();
                    _config.ResetConditions.Satisfied += ResetConditionsOnSatisfied;
                    SetupTimerActions();
                    CheckDailyReset();
                    if(_config.LockedPauseEnabled) SystemEvents.SessionSwitch += SysEventsCheck;
                }
            }
        }

        #region OtherFields

        private Window TimerWindow { get;  set; }
        public Brush TempBackgroundBrush { get; set; }
        public Brush TempTextBrush { get; set; }

        private readonly WeakEventSource<EventArgs> _finished = new();

        public event EventHandler<EventArgs> Finished {
            add => _finished.Subscribe(value);
            remove => _finished.Unsubscribe(value);
        }

        private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
        private readonly DispatcherTimer _timer = new() {Interval = OneSecond};
        private bool _finishedSet;

        private bool _lockPauseActive;
        private bool _inactivityPauseActive;
        private readonly DispatcherTimer _activityCheckTimer = new() {Interval = TimeSpan.FromSeconds(5)};
        private bool CanResume => !_lockPauseActive && !_inactivityPauseActive;
        private bool IsOrWasRunning => _timer.IsEnabled || _lockPauseActive || _inactivityPauseActive;

        public bool Running => _timer.IsEnabled;

        #endregion

        public TimerModel(TimerConfig config) {
            Config = config;

            _timer.Tick += OnTimerOnTick;
            _activityCheckTimer.Tick += ActivityCheckTimerOnTick;

            VDM.DesktopChanged += (_, _) => HandleDesktopChanged();
        }

        public ITimer Duplicate() {
            var copyconfig = Config.DeepCopy();
            copyconfig.InstanceID = Guid.NewGuid();
            copyconfig.ZeroOverflowAction.TimerId = copyconfig.InstanceID;
            foreach(OverflowAction a in copyconfig.OverflowActions) a.TimerId = copyconfig.InstanceID;
            foreach(ResetCondition c in copyconfig.ResetConditions) c.TimerId = copyconfig.InstanceID;

            return new TimerModel(copyconfig);
        }


        public void CheckStart() {
            if(!Config.Enabled) return;
            if(Config.TimeLeft <= TimeSpan.Zero) TimerFinishedRaise();
            if(Config.StartResetConditionsEarly && Config.TimeLeft < Config.Duration)
                Config.ResetConditions.StartConditions();
        }

        private void OnTimerOnTick(object o, EventArgs e) {
            Config.TimeLeft -= OneSecond;
            CheckTimerActions();
            InactivityCheck();
        }

        private void TimerFinishedRaise() {
            if(_finishedSet) return;
            _finishedSet = true;
            _finished.Raise(this, EventArgs.Empty);
            if(!Config.OverflowEnabled) StopTimer();
            Config.ResetConditions.StartConditions();
        }

        #region TimerActions

        /// <summary> Add a timer action. WARNING: May not be re-added when program opened or config changed </summary>
        public void AddTimerAction(TimerAction action, bool isInsideAction) {
            _timerActions.Add(action);
            FixTimerActionOrder();
            if(isInsideAction) _taPointer--;
        }

        private readonly List<TimerAction> _timerActions = new();
        private int _taPointer;

        /// <summary> Add all timer actions and move pointer to right place. Idempotent. </summary>
        private void SetupTimerActions() {
            _timerActions.Clear();

            _timerActions.Add(new TimerAction(TimeSpan.Zero, TimerFinishedRaise));
            if(Config.StartResetConditionsEarly)
                _timerActions.Add(new TimerAction(Config.Duration - TimeSpan.FromSeconds(1),
                                                  Config.ResetConditions.StartConditions));

            _timerActions.Add(new TimerAction(TimeSpan.Zero, Config.ZeroOverflowAction.DoAction));
            foreach(var x in Config.OverflowActions) {
                TimeSpan initTime = TimeSpan.FromMinutes(-x.AfterMinutes);
                _timerActions.Add(new TimerAction(initTime, x.DoAction));
                if(x.RepeatEnabled && Config.TimeLeft <= initTime) {
                    double repeatTimes = Math.Ceiling((-x.AfterMinutes - Config.TimeLeft.TotalMinutes) / x.RepeatMinutes);
                    TimeSpan nextRepeat = initTime - repeatTimes * TimeSpan.FromMinutes(x.RepeatMinutes);
                    _timerActions.Add(new TimerAction(nextRepeat, x.DoAction));
                }
            }

            FixTimerActionOrder();
        }

        private void FixTimerActionOrder() {
            _timerActions.Sort();
            _taPointer = 0;
            while(_taPointer < _timerActions.Count && Config.TimeLeft <= _timerActions[_taPointer].TriggerTime)
                _taPointer++;
        }

        private void CheckTimerActions() {
            while(_taPointer < _timerActions.Count &&
                  Config.TimeLeft <= _timerActions[_taPointer].TriggerTime) {
                _timerActions[_taPointer].Action();
                _taPointer++;
            }
        }

        #endregion

        #region VirtualDesktops

        private void HandleDesktopChanged() {
            if(!Config.Enabled) return;
            int newDesktop = VDM.CurrentDesktop();
            TimerWindow?.Dispatcher.Invoke(() => {
                Config.DesktopsVisible ??= [-1];
                if((Config.DesktopsVisible.Contains(-1) || Config.DesktopsVisible.Contains(newDesktop)) && Config.Visible &&
                   TimersService.Singleton.VisState != VisibilityState.Hidden) {
                    TimerWindow.Visibility = Visibility.Visible;
                    VDM.MoveToDesktop(TimerWindow, newDesktop);
                } else if(Config.DesktopsVisible.Count > 0)  {
                    TimerWindow.Visibility = Visibility.Hidden;
                }
            });

            StartStopForDesktopsActive();
        }

        public void StartStopForDesktopsActive() {
            if(Config.DesktopsActive == null || Config.DesktopsActive.Count == 0) return;
            if(Config.DesktopsActive.Contains(-1) || Config.DesktopsActive.Contains(VDM.CurrentDesktop())) StartTimer();
            else StopTimer();
        }

        #endregion


        public void ToggleLockPosition() => Config.PositionIsLocked = !Config.PositionIsLocked;


        #region VisibilityStuff

        public void ShowTimer(bool shouldActivate = false) {
            if(!Config.Enabled) return;
            if(TimerWindow != null) {
                if(shouldActivate) TimerWindow.Activate();
                return;
            }

            TimerWindow = new TimerWindow {Content = new TimerView(new TimerViewModel(this)), Title = Config.Name};
            TimerWindow.Show();
            TimerWindow.Closed += (_,  _) => {
                SaveSettings();
                TimerWindow = null;
            };

            switch(TimersService.Singleton.VisState) {
                case VisibilityState.MoveBehind:
                    SetBottommost();
                    break;
                default:
                    SetTopmost();
                    break;
            }
            HandleDesktopChanged();
        }

        public void SetVisibility(bool vis) {
            if(!Config.Enabled) return;
            Config.Visible = vis;
            if(Config.Visible) ShowTimer();
            else TimerWindow?.Close();
        }

        public void ToggleVisibility() => SetVisibility(!Config.Visible);

        public void SetTopmost() {
            if(!(Config.Enabled && Config.Visible)) return;
            TimerWindow.Topmost = true;
        }

        public void SetBottommost() {
            if(!(Config.Enabled && Config.Visible)) return;
            TimerWindow.Topmost = false;

            var hWnd = new WindowInteropHelper(TimerWindow).Handle;
            SetWindowPos(hWnd, new IntPtr(1), 0, 0, 0, 0, 19U);
        }

        [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y,
                                                                          int cx, int cy, uint uFlags);

        #endregion

        #region Disable

        private void SetEnabled(bool set) {
            Config.Enabled = set;
            if(Config.Enabled) {
                Config = _config; //resubscribe to whatever is needed
                CheckStart();
                if(Config.Visible) ShowTimer();
            } else {
                StopTimer();
                UnregisterShortcuts();
                _activityCheckTimer.Stop();
                _inactivityPauseActive = false;
                _config.ResetConditions.Satisfied -= ResetConditionsOnSatisfied;
                SystemEvents.SessionSwitch -= SysEventsCheck;
                _dailyResetTimer?.Dispose();
                TimerWindow?.Close();
            }
        }

        public void ToggleEnabled() => SetEnabled(!Config.Enabled);

        #endregion

        #region Reset

        private void ResetTimer() {
            Config.TimeLeft = Config.Duration;
            _finishedSet = false;
            _taPointer = 0;
            Config.ResetConditions.StopAndResetAllConditions();
        }

        public void RequestResetTimer() {
            if(!Config.Enabled) return;
            if(_finishedSet && Config.ResetConditions.IsSat() || !_finishedSet && Config.AllowResetWhileRunning) {
                ResetTimer();
                return;
            }

            string unmetStrings = _finishedSet ? Config.ResetConditions.UnmetStrings() : "The timer has not finished yet.";

            string  message = $"Not all reset conditions are met:\n\n<Bold>{unmetStrings}</Bold>";
            if(Config.AllowResetOverride) {
                IDialogResult r = Dialogs.ShowNotification(message + "\n\nDo you want to override?",
                                                           $"Resetting {Config.Name}", hasCancel: true,
                                                           customOk: "YES", customCancel: "NO").Result;
                if(r.Result == ButtonResult.OK) ResetTimer();
            } else {
                Dialogs.ShowNotification(message, $"Unable to Reset {Config.Name}");
            }
        }

        private void ResetConditionsOnSatisfied(object sender, EventArgs e) {
            if(!Config.AutoResetOnConditions) return;
            ResetTimer();
            StartStopForDesktopsActive();
        }

        #endregion

        #region StartStop

        public void StartTimer() {
            if(!Config.Enabled || Config.TimeLeft.TotalSeconds <= 0 && !Config.OverflowEnabled) return;
            _timer.Start();
            this.OnPropertyChanged(); // Notify the viewmodel to change the background if needed
            TimersService.Singleton.isStopAll = false;
        }

        public void StopTimer() {
            _timer.Stop();
            this.OnPropertyChanged(); // Notify the viewmodel to change the background if needed
        }

        #endregion

        #region DailyReset

        private AbsoluteTimer.AbsoluteTimer _dailyResetTimer;

        private void CheckDailyReset(bool newConfig = true) {
            if(!Config.DailyResetEnabled) {
                _dailyResetTimer?.Dispose();
                return;
            }

            DateTime now = DateTime.Now;
            if(now > Config.DailyResetTime) {
                if(!newConfig || now > Config.DailyResetTime.AddDays(1)) ResetTimer();

                Config.DailyResetTime = DateTime.Today.AddMinutes(Config.DailyResetTime.TimeOfDay.TotalMinutes);

                // If it's already past DailyResetTime, wait until DailyResetTime tomorrow    
                if(now > Config.DailyResetTime) Config.DailyResetTime = Config.DailyResetTime.AddDays(1.0);
            }

            _dailyResetTimer?.Dispose();
            _dailyResetTimer = new AbsoluteTimer.AbsoluteTimer(Config.DailyResetTime, _ => CheckDailyReset(false), null);
        }

        #endregion

        #region Settings

        public async Task<ButtonResult> OpenSettings() {
            bool wasRunning = Running;
            StopTimer();
            IDialogResult r = await Dialogs.ShowDialogAsync(nameof(TimerSettingsView),
                                                            new DialogParameters {{nameof(ITimer), this}});
            if(wasRunning) StartTimer();
            return r.Result;
        }

        public void SaveSettings() => TimersService.Singleton.SaveSettings();

        #endregion

        #region Shortcuts

        private enum TimerHotkeyState { ShouldStart, ShouldStop }

        public void RegisterShortcuts() { RegisterShortcuts(Config); }

        public void RegisterShortcuts(TimerConfig timerConfig) {
            HotkeyManager.RegisterHotkey(timerConfig.InstanceID, timerConfig, nameof(timerConfig.ResetShortcut),
                                         RequestResetTimer, CompatibilityType.Reset);

            int NextTimerState(int r) => (int) (Running ? TimerHotkeyState.ShouldStop : TimerHotkeyState.ShouldStart);
            HotkeyManager.RegisterHotkey(timerConfig.InstanceID, timerConfig, nameof(timerConfig.StartShortcut), StartTimer,
                                         CompatibilityType.StartStop, (int) TimerHotkeyState.ShouldStart, NextTimerState);
            HotkeyManager.RegisterHotkey(timerConfig.InstanceID, timerConfig, nameof(timerConfig.StopShortcut), StopTimer,
                                         CompatibilityType.StartStop, (int) TimerHotkeyState.ShouldStop, NextTimerState);

            HotkeyManager.RegisterHotkey(timerConfig.InstanceID, timerConfig, nameof(timerConfig.ToggleVisibilityShortcut),
                                         ToggleVisibility, CompatibilityType.Visibility);
        }

        private void UnregisterShortcuts() {
            HotkeyManager.UnregisterHotkey(Config.InstanceID, nameof(Config.ResetShortcut));
            HotkeyManager.UnregisterHotkey(Config.InstanceID, nameof(Config.StartShortcut));
            HotkeyManager.UnregisterHotkey(Config.InstanceID, nameof(Config.StopShortcut));
            HotkeyManager.UnregisterHotkey(Config.InstanceID, nameof(Config.ToggleVisibilityShortcut));
        }

        #endregion

        #region AdjustTime

        public void SetTime(TimeSpan time) {
            if(_finishedSet && time > TimeSpan.Zero) {
                _finishedSet = false;
                if(!Config.StartResetConditionsEarly) Config.ResetConditions.StopAndResetAllConditions();
            }
            if(!_finishedSet && time <= TimeSpan.Zero) TimerFinishedRaise();
            Config.TimeLeft = time;
            FixTimerActionOrder();
        }

        #endregion

        #region LockChecking

        private void SysEventsCheck(object sender, SessionSwitchEventArgs e) {
            switch(e.Reason) {
                case SessionSwitchReason.SessionLock:
                    if(!IsOrWasRunning) return;
                    _lockPauseActive = true;
                    StopTimer();
                    break;
                case SessionSwitchReason.SessionUnlock:
                    if(!_lockPauseActive) return;
                    _lockPauseActive = false;
                    if(CanResume) StartTimer();
                    break;
            }
        }

        #endregion

        #region InactivityChecking

        [StructLayout(LayoutKind.Sequential)]
        private struct LastInputInfo {
            public uint cbSize;
            public readonly uint dwTime;
        }

        [DllImport("user32.dll")] private static extern bool GetLastInputInfo(ref LastInputInfo plii);

        private static double GetInactiveMinutes() {
            LastInputInfo info = new();
            info.cbSize = (uint) Marshal.SizeOf(info);
            const double millisecondsToMinutes = 60000;
            return GetLastInputInfo(ref info) ? (Environment.TickCount - info.dwTime) / millisecondsToMinutes : 0;
        }

        private void InactivityCheck() {
            if(Config.InactivityPauseEnabled && GetInactiveMinutes() > Config.InactivityMinutes) {
                _inactivityPauseActive = true;
                StopTimer();
                _activityCheckTimer.Start();
            }
        }

        private void ActivityCheckTimerOnTick(object sender, EventArgs e) {
            if(GetInactiveMinutes() > Config.InactivityMinutes) return;

            _inactivityPauseActive = false;
            if(CanResume) StartTimer();
            _activityCheckTimer.Stop();
        }

        #endregion

        public override bool Equals(object obj) => obj is ITimer other && other.Config.InstanceID.Equals(Config.InstanceID);
        public override int GetHashCode() => Config.InstanceID.GetHashCode();
        public override string ToString() => Config.Name;

        // To detect redundant calls
        private bool _disposed;

        public void Dispose() {
            if(_disposed) return;
            SetEnabled(false);

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
