using System;
using System.Windows.Threading;
using Infrastructure.Constants;
using Infrastructure.SharedResources;
using Prio.GlobalServices;
using Prism.Ioc;
using Prism.Services.Dialogs;

namespace Timer {
    public class TimerModel : NotifyPropertyWithDependencies, ITimer {
        private TimerConfig _config;

        public TimerConfig Config {
            get => _config;
            set => NotificationBubbler.BubbleSetter(ref _config, value, (o, e) => this.OnPropertyChanged());
        }


        private readonly IDialogService _dialogService;
        private readonly DispatcherTimer _timer;
        public bool IsRunning => _timer.IsEnabled;

        public TimerModel(TimerConfig config) {
            Config = config;
            _dialogService = UnityInstance.GetContainer().Resolve<IDialogService>();

            _timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            _timer.Tick += (o,  e) => Config.TimeLeft -= TimeSpan.FromSeconds(1);

            RequestHide = () => {
                Config.VisibilityState = VisibilityState.Hidden;
                SaveSettings();
            };
            RequestKeepOnTop = () => {
                Config.VisibilityState = VisibilityState.KeepOnTop;
                SaveSettings();
            };
            RequestKeepBehind = () => {
                Config.VisibilityState = VisibilityState.MoveBehind;
                SaveSettings();
            };

            RegisterShortcuts();
        }


        public void ShowTimer() =>
            _dialogService.Show(nameof(TimerView), new DialogParameters {{nameof(ITimer), this}}, result => { });

        public void ResetTimer() => Config.TimeLeft = Config.Duration;
        public void StartTimer() => _timer.Start();
        public void StopTimer() => _timer.Stop();
        private event Action RequestHide;
        private event Action RequestKeepOnTop;
        private event Action RequestKeepBehind;

        #region VisibilityEvents

        event Action ITimer.RequestHide {
            add {
                RequestHide += value;
                RegisterShortcuts();
            }
            remove {
                RequestHide -= value;
                RegisterShortcuts();
            }
        }

        event Action ITimer.RequestKeepOnTop {
            add {
                RequestKeepOnTop += value;
                RegisterShortcuts();
            }
            remove {
                RequestKeepOnTop -= value;
                RegisterShortcuts();
            }
        }

        event Action ITimer.RequestMoveBelow {
            add {
                RequestKeepBehind += value;
                RegisterShortcuts();
            }
            remove {
                RequestKeepBehind -= value;
                RegisterShortcuts();
            }
        }

        #endregion

        public void OpenSettings() {
            StopTimer();
            _dialogService.ShowDialog(nameof(TimerSettingsView), new DialogParameters {{nameof(ITimer), this}},
                                      result => { });
        }

        public void SaveSettings() {
            Settings.SaveSettings(Config, ModuleNames.TIMER, Config.InstanceID);
            RegisterShortcuts();
        }

        private enum TimerHotkeyState { ShouldStart, ShouldStop }

        private enum VisibilityHotkeyState { ShouldHide, ShouldTop, ShouldBehind }

        private void RegisterShortcuts() {
            IContainerProvider container = UnityInstance.GetContainer();
            var hotkeyManager = container.Resolve<IPrioHotkeyManager>();
            hotkeyManager.RegisterHotkey(Config.InstanceID, nameof(Config.ResetShortcut), Config.ResetShortcut, ResetTimer,
                                         CompatibilityType.Reset);

            int NextTimerState() => (int) (IsRunning ? TimerHotkeyState.ShouldStop : TimerHotkeyState.ShouldStart);
            hotkeyManager.RegisterHotkey(Config.InstanceID, nameof(Config.StartShortcut), Config.StartShortcut, StartTimer,
                                         CompatibilityType.StartStop, (int) TimerHotkeyState.ShouldStart, NextTimerState);
            hotkeyManager.RegisterHotkey(Config.InstanceID, nameof(Config.StopShortcut), Config.StopShortcut, StopTimer,
                                         CompatibilityType.StartStop, (int) TimerHotkeyState.ShouldStop, NextTimerState);


            int NextVisibilityState() {
                switch(Config.VisibilityState) {
                    case VisibilityState.Hidden:
                        if(Equals(Config.ShowHideShortcut, Config.KeepOnTopShortcut))
                            return (int) VisibilityHotkeyState.ShouldTop;
                        else if(Equals(Config.ShowHideShortcut, Config.MoveBehindShortcut))
                            return (int) VisibilityHotkeyState.ShouldBehind;
                        return (int) VisibilityHotkeyState.ShouldHide;
                    case VisibilityState.KeepOnTop:
                        if(Equals(Config.KeepOnTopShortcut, Config.MoveBehindShortcut))
                            return (int) VisibilityHotkeyState.ShouldBehind;
                        else if(Equals(Config.KeepOnTopShortcut, Config.ShowHideShortcut))
                            return (int) VisibilityHotkeyState.ShouldHide;
                        return (int) VisibilityHotkeyState.ShouldTop;
                    case VisibilityState.MoveBehind:
                        if(Equals(Config.MoveBehindShortcut, Config.ShowHideShortcut))
                            return (int) VisibilityHotkeyState.ShouldHide;
                        else if(Equals(Config.MoveBehindShortcut, Config.KeepOnTopShortcut))
                            return (int) VisibilityHotkeyState.ShouldTop;
                        return (int) VisibilityHotkeyState.ShouldBehind;
                }
                return (int) (IsRunning ? TimerHotkeyState.ShouldStop : TimerHotkeyState.ShouldStart);
            }

            hotkeyManager.RegisterHotkey(Config.InstanceID, nameof(Config.ShowHideShortcut), Config.ShowHideShortcut,
                                         RequestHide, CompatibilityType.Visibility,
                                         (int) VisibilityHotkeyState.ShouldHide, NextVisibilityState);
            hotkeyManager.RegisterHotkey(Config.InstanceID, nameof(Config.KeepOnTopShortcut), Config.KeepOnTopShortcut,
                                         RequestKeepOnTop, CompatibilityType.Visibility,
                                         (int) VisibilityHotkeyState.ShouldTop, NextVisibilityState);
            hotkeyManager.RegisterHotkey(Config.InstanceID, nameof(Config.MoveBehindShortcut), Config.MoveBehindShortcut,
                                         RequestKeepBehind, CompatibilityType.Visibility,
                                         (int) VisibilityHotkeyState.ShouldBehind, NextVisibilityState);
        }
    }
}