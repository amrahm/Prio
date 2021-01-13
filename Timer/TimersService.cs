using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Infrastructure.Constants;
using Infrastructure.SharedResources;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Prio.GlobalServices;
using Prism.Ioc;

namespace Timer {
    public class TimersService : NotifyPropertyChanged {
        public static readonly TimersService Singleton = new();

        public TimersGeneralConfig GeneralConfig { get; set; }

        public ObservableHashSet<ITimer> Timers { get; } = new();
        public ITimer GetTimer(Guid id) => Timers.FirstOrDefault(x => x.Config.InstanceID == id);


        private readonly DispatcherTimer _autosaveTimer = new() {Interval = TimeSpan.FromMinutes(5)};

        private TimersService() {
            GeneralConfig = Settings.LoadSettings<TimersGeneralConfig>(ModuleNames.TIMER) ?? new TimersGeneralConfig();
            foreach(TimerConfig config in GeneralConfig.TimerConfigs) Timers.Add(new TimerModel(config));

            RegisterShortcuts();
            currVisState = GeneralConfig.DefaultVisibilityState;

            _autosaveTimer.Tick += (_,  _) => SaveSettings();
            _autosaveTimer.Start();
        }

        public void ShowTimers() => Timers.ForEach(t => {
            t.ShowTimer();
            t.CheckStart();
        });

        public void SaveSettings() {
            GeneralConfig.TimerConfigs = Timers.Select(t => t.Config).ToList();
            Settings.SaveSettings(GeneralConfig, ModuleNames.TIMER);
            RegisterShortcuts();
        }

        #region VisibilityHotkeyStuff

        public VisibilityState currVisState;
        private VisibilityState _lastNonHiddenVisState = VisibilityState.KeepOnTop;

        public void ApplyVisState() {
            switch(currVisState) {
                case VisibilityState.KeepOnTop:
                    TopAll();
                    break;
                case VisibilityState.MoveBehind:
                    BottomAll();
                    break;
                case VisibilityState.Hidden:
                    ShowHideAll();
                    break;
            }
        }

        private void ShowHideAll() {
            Visibility target;
            if(currVisState != VisibilityState.Hidden) {
                target = Visibility.Hidden;
                currVisState = VisibilityState.Hidden;
            } else {
                target = Visibility.Visible;
                currVisState = _lastNonHiddenVisState;
            }
            foreach(ITimer timer in Timers)
                if(timer.TimerWindow != null) {
                    timer.TimerWindow.Visibility = target;
                    if(target == Visibility.Visible) timer.TimerWindow.Activate();
                }
        }

        private void TopAll() {
            currVisState = VisibilityState.KeepOnTop;
            _lastNonHiddenVisState = VisibilityState.KeepOnTop;
            foreach(ITimer timer in Timers) {
                if(timer.TimerWindow != null) {
                    timer.TimerWindow.Visibility = Visibility.Visible;
                    timer.TimerWindow.Topmost = true;
                }
            }
        }

        private void BottomAll() {
            currVisState = VisibilityState.MoveBehind;
            _lastNonHiddenVisState = VisibilityState.MoveBehind;
            foreach(ITimer timer in Timers) {
                if(timer.TimerWindow != null) {
                    timer.TimerWindow.Visibility = Visibility.Visible;
                    timer.TimerWindow.Topmost = false;

                    var hWnd = new WindowInteropHelper(timer.TimerWindow).Handle;
                    SetWindowPos(hWnd, new IntPtr(1), 0, 0, 0, 0, 19U);
                }
            }
        }

        [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y,
                                                                          int cx, int cy, uint uFlags);

        private enum VisibilityHotkeyState { ShouldHide, ShouldTop, ShouldBehind }

        #endregion

        private void RegisterShortcuts() {
            IContainerProvider container = UnityInstance.GetContainer();
            var hotkeyManager = container.Resolve<IPrioHotkeyManager>();

            bool hideIsTop = Equals(GeneralConfig.ShowHideTimersShortcut, GeneralConfig.KeepTimersOnTopShortcut);
            bool hideIsBottom = Equals(GeneralConfig.ShowHideTimersShortcut, GeneralConfig.MoveTimersBehindShortcut);
            bool topIsBottom = Equals(GeneralConfig.MoveTimersBehindShortcut, GeneralConfig.KeepTimersOnTopShortcut);

            int NextVisibilityState(int r) {
                bool isHidden = currVisState == VisibilityState.Hidden;
                bool isTop = currVisState == VisibilityState.KeepOnTop;
                bool isBottom = currVisState == VisibilityState.MoveBehind;
                switch((VisibilityHotkeyState) r) { //Find all cases where we shouldn't do the requested action
                    // These set precedence so that we move in a triangle if needed
                    // They also check if we are requesting to do what we already are
                    case VisibilityHotkeyState.ShouldHide when hideIsBottom && isTop:
                    case VisibilityHotkeyState.ShouldTop when isTop || hideIsTop && isBottom:
                    case VisibilityHotkeyState.ShouldBehind when isBottom || topIsBottom && isHidden:

                    // If we are currently in a showhide state, however, we might want to repeat the action
                    // But only if there isn't another visibility action with the same key
                    case VisibilityHotkeyState.ShouldHide when isHidden && (hideIsTop || hideIsBottom):
                        return -1;
                }
                return r; //otherwise, do the requested action
            }

            hotkeyManager.RegisterHotkey(nameof(GeneralConfig.ShowHideTimersShortcut), GeneralConfig.ShowHideTimersShortcut,
                                         ShowHideAll, CompatibilityType.Visibility,
                                         (int) VisibilityHotkeyState.ShouldHide, NextVisibilityState);

            hotkeyManager.RegisterHotkey(nameof(GeneralConfig.KeepTimersOnTopShortcut),
                                         GeneralConfig.KeepTimersOnTopShortcut,
                                         TopAll, CompatibilityType.Visibility,
                                         (int) VisibilityHotkeyState.ShouldTop, NextVisibilityState);

            hotkeyManager.RegisterHotkey(nameof(GeneralConfig.MoveTimersBehindShortcut),
                                         GeneralConfig.MoveTimersBehindShortcut,
                                         BottomAll, CompatibilityType.Visibility,
                                         (int) VisibilityHotkeyState.ShouldBehind, NextVisibilityState);


            int NextTimerState(int r) => (int) (isStopAll ? TimerHotkeyState.ShouldStart : TimerHotkeyState.ShouldStop);
            hotkeyManager.RegisterHotkey(nameof(GeneralConfig.StopAllShortcut), GeneralConfig.StopAllShortcut, StopAll,
                                         CompatibilityType.StartStop, (int) TimerHotkeyState.ShouldStop, NextTimerState);
            hotkeyManager.RegisterHotkey(nameof(GeneralConfig.ResumeAllShortcut), GeneralConfig.ResumeAllShortcut, ResumeAll,
                                         CompatibilityType.StartStop, (int) TimerHotkeyState.ShouldStart, NextTimerState);
        }

        private void StopAll() {
            isStopAll = true;
            _stoppedTimers.Clear();
            foreach(ITimer timer in Timers) {
                if(timer.IsRunning) {
                    timer.StopTimer();
                    _stoppedTimers.Push(timer);
                }
            }
        }

        private void ResumeAll() {
            isStopAll = false;
            while(_stoppedTimers.Count > 0) _stoppedTimers.Pop().StartTimer();
        }

        private enum TimerHotkeyState { ShouldStart, ShouldStop }

        public bool isStopAll;
        private readonly Stack<ITimer> _stoppedTimers = new();
    }
}