using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Infrastructure.Constants;
using Infrastructure.SharedResources;
using Prio.GlobalServices;
using Prism.Services.Dialogs;
using static Infrastructure.SharedResources.UnityInstance;

namespace Timer {
    public class TimersService : NotifyPropertyChanged {
        public static readonly TimersService Singleton = new();
        public static TimersGeneralConfig Config => Singleton.Conf;

        public TimersGeneralConfig Conf { get; set; }

        public ObservableCollection<ITimer> Timers { get; } = new();
        public ITimer GetTimer(Guid id) => Timers.FirstOrDefault(x => x.Config.InstanceID == id);


        private readonly DispatcherTimer _autosaveTimer = new() {Interval = TimeSpan.FromMinutes(5)};

        private TimersService() {
            Conf = Settings.LoadSettings<TimersGeneralConfig>(ModuleNames.TIMER) ?? new TimersGeneralConfig();
            foreach(TimerConfig config in Conf.TimerConfigs) Timers.Add(new TimerModel(config));

            RegisterShortcuts(Conf);

            _autosaveTimer.Tick += (_,  _) => SaveSettings();
            _autosaveTimer.Start();

            Application.Current.Exit += (_,  _) => SaveSettings();
        }

        public void ShowTimersAtStartup() {
            // This temp stuff is needed in case visibililty is set to hidden
            // Since for some reason that crashes the virtual desktop package
            var actualVis = Conf.CurrVisState;
            ApplyVisState(VisibilityState.KeepOnTop);
            ApplyVisState(actualVis);
            Timers.ForEach(t => t.CheckStart());
        }

        public void SaveSettings() {
            Conf.TimerConfigs = Timers.Select(t => t.Config).ToList();
            Settings.SaveSettings(Conf, ModuleNames.TIMER);
            RegisterShortcuts(Conf);
        }

        public void DeleteTimer(Guid id) {
            ITimer timer = GetTimer(id);
            var r = Dialogs.ShowNotification("Are you sure you want to delete this timer?\n\nThis cannot be undone.",
                                             $"Deleting {timer.Config.Name}", hasCancel: true, customOk: "YES",
                                             customCancel: "NO").Result;

            if(r.Result == ButtonResult.OK) {
                Timers.Remove(timer);
                Conf.TimerConfigs.Remove(timer.Config);
                timer.Dispose();
                SaveSettings();
            }
        }

        #region VisibilityHotkeyStuff

        private VisibilityState _lastNonHiddenVisState = VisibilityState.KeepOnTop;

        private void SetShowHide(bool show) {
            Conf.CurrVisState = show ? _lastNonHiddenVisState : VisibilityState.Hidden;
            foreach(ITimer timer in Timers) timer.SetVisibility(Conf.CurrVisState != VisibilityState.Hidden);
        }
        private void ShowHideAll() => SetShowHide(Conf.CurrVisState == VisibilityState.Hidden);

        public void TopAll() {
            Conf.CurrVisState = _lastNonHiddenVisState = VisibilityState.KeepOnTop;
            foreach(ITimer timer in Timers) {
                timer.SetVisibility(true);
                timer.SetTopmost();
            }
        }

        public void BottomAll() {
            Conf.CurrVisState = _lastNonHiddenVisState = VisibilityState.MoveBehind;
            foreach(ITimer timer in Timers) {
                timer.SetVisibility(true);
                timer.SetBottommost();
            }
        }

        public void ApplyVisState(VisibilityState? visState = null) {
            if(visState.HasValue) Conf.CurrVisState = visState.Value;
            switch(Conf.CurrVisState) {
                case VisibilityState.KeepOnTop:
                    TopAll();
                    break;
                case VisibilityState.MoveBehind:
                    BottomAll();
                    break;
                case VisibilityState.Hidden:
                    SetShowHide(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private enum VisibilityHotkeyState { ShouldHide, ShouldTop, ShouldBehind }

        #endregion

        public void RegisterShortcuts(TimersGeneralConfig config) {
            bool hideIsTop = Equals(config.ToggleVisibilityShortcut, config.KeepTimersOnTopShortcut);
            bool hideIsBottom = Equals(config.ToggleVisibilityShortcut, config.MoveTimersBehindShortcut);
            bool topIsBottom = Equals(config.MoveTimersBehindShortcut, config.KeepTimersOnTopShortcut);

            int NextVisibilityState(int r) {
                bool isHidden = Conf.CurrVisState == VisibilityState.Hidden;
                bool isTop = Conf.CurrVisState == VisibilityState.KeepOnTop;
                bool isBottom = Conf.CurrVisState == VisibilityState.MoveBehind;
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

            HotkeyManager.RegisterHotkey(config, nameof(config.ToggleVisibilityShortcut), ShowHideAll,
                                         CompatibilityType.Visibility, (int) VisibilityHotkeyState.ShouldHide,
                                         NextVisibilityState);

            HotkeyManager.RegisterHotkey(config, nameof(config.KeepTimersOnTopShortcut),
                                         TopAll, CompatibilityType.Visibility, (int) VisibilityHotkeyState.ShouldTop,
                                         NextVisibilityState);

            HotkeyManager.RegisterHotkey(config, nameof(config.MoveTimersBehindShortcut),
                                         BottomAll, CompatibilityType.Visibility, (int) VisibilityHotkeyState.ShouldBehind,
                                         NextVisibilityState);


            int NextTimerState(int r) => (int) (isStopAll ? TimerHotkeyState.ShouldStart : TimerHotkeyState.ShouldStop);
            HotkeyManager.RegisterHotkey(config, nameof(config.StopAllShortcut), StopAll,
                                         CompatibilityType.StartStop, (int) TimerHotkeyState.ShouldStop, NextTimerState);
            HotkeyManager.RegisterHotkey(config, nameof(config.ResumeAllShortcut), ResumeAll,
                                         CompatibilityType.StartStop, (int) TimerHotkeyState.ShouldStart, NextTimerState);
        }

        private void StopAll() {
            isStopAll = true;
            _stoppedTimers.Clear();
            foreach(ITimer timer in Timers) {
                if(timer.Running) {
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