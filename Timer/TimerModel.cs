using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
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
                NotificationBubbler.BubbleSetter(ref _config, value, (o, e) => this.OnPropertyChanged());
                if(_config != null) _config.ResetConditions.Satisfied += ResetConditionsOnSatisfied;
            }
        }

        public Window TimerWindow { get; private set; }

        private readonly WeakEventSource<EventArgs> _finished = new WeakEventSource<EventArgs>();
        public event EventHandler<EventArgs> Finished {
            add => _finished.Subscribe(value);
            remove => _finished.Unsubscribe(value);
        }

        private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
        private readonly IDialogService _dialogService;
        private readonly DispatcherTimer _timer = new DispatcherTimer {Interval = OneSecond};
        private bool _hidden;
        private bool _finishedSet;
        private readonly IVirtualDesktopManager _vdm;

        public bool IsRunning => _timer.IsEnabled;

        public TimerModel(TimerConfig config) {
            Config = config;
            IContainerProvider container = UnityInstance.GetContainer();
            _dialogService = container.Resolve<IDialogService>();

            _timer.Tick += OnTimerOnTick;

            RegisterShortcuts();

            _vdm = container.Resolve<IVirtualDesktopManager>();
            _vdm.DesktopChanged += (o, e) => HandleDesktopChanged(e.NewDesktop);
        }

        public void CheckStart() => TimerFinishCheckRaise();

        private void OnTimerOnTick(object o, EventArgs e) {
            Config.TimeLeft -= OneSecond;
            TimerFinishCheckRaise();
        }

        private void TimerFinishCheckRaise() {
            if(!_finishedSet && Config.TimeLeft.TotalSeconds <= 0.01) {
                _finishedSet = true;
                _finished.Raise(this, EventArgs.Empty);
                if(!Config.OverflowEnabled) StopTimer();
                Config.ResetConditions.StartConditions();
            }
        }

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


        public void ShowTimer() {
            if(TimerWindow != null) {
                TimerWindow.Activate();
                return;
            }

            TimerWindow = new TimerWindow {Content = new TimerView(new TimerViewModel(this)), Title = Config.Name};
            TimerWindow.Show();
            TimerWindow.Closed += (o,  e) => {
                SaveSettings();
                TimerWindow = null;
            };

            TimersService.Singleton.ApplyVisState();
            HandleDesktopChanged(_vdm.CurrentDesktop());
        }

        private void ResetTimer() {
            Config.TimeLeft = Config.Duration;
            _finishedSet = false;
            Config.ResetConditions.StopAllConditions();
        }

        public void RequestResetTimer() {
            if(!_finishedSet && Config.AllowResetWhileRunning || Config.ResetConditions.IsSat()) {
                ResetTimer();
                return;
            }

            string unmetStrings = _finishedSet ? Config.ResetConditions.UnmetStrings() : "The timer has not finished yet.";

            if(Config.AllowResetOverride) {
                MessageBoxResult result = MessageBox.Show(
                    $"Not all reset conditions are met:\n\n{unmetStrings}\n\nDo you want to override?",
                    $"Resetting: {Config.Name}", MessageBoxButton.YesNo);
                if(result == MessageBoxResult.Yes) ResetTimer();
            } else {
                MessageBox.Show($"Not all reset conditions are met:\n{unmetStrings}");
            }
        }

        public void StartTimer() {
            if(Config.OverflowEnabled || Config.TimeLeft.TotalSeconds > 0) _timer.Start();
        }

        public void StopTimer() => _timer.Stop();

        private void ShowHideTimer() {
            if(TimerWindow == null) return;
            if(_hidden) {
                TimerWindow.Visibility = Visibility.Visible;
                TimerWindow.Activate();
            } else TimerWindow.Visibility = Visibility.Hidden;
            _hidden = !_hidden;
        }

        public async Task<ButtonResult> OpenSettings() {
            StopTimer();
            IDialogResult r = await _dialogService.ShowDialogAsync(nameof(TimerSettingsView),
                                                                   new DialogParameters {{nameof(ITimer), this}});
            return r.Result;
        }

        public void SaveSettings() {
            TimersService.Singleton.SaveSettings();
            RegisterShortcuts();
        }

        private enum TimerHotkeyState { ShouldStart, ShouldStop }

        private void RegisterShortcuts() {
            IContainerProvider container = UnityInstance.GetContainer();
            var hotkeyManager = container.Resolve<IPrioHotkeyManager>();
            hotkeyManager.RegisterHotkey(Config.InstanceID, nameof(Config.ResetShortcut), Config.ResetShortcut,
                                         RequestResetTimer,
                                         CompatibilityType.Reset);

            int NextTimerState(int r) => (int) (IsRunning ? TimerHotkeyState.ShouldStop : TimerHotkeyState.ShouldStart);
            hotkeyManager.RegisterHotkey(Config.InstanceID, nameof(Config.StartShortcut), Config.StartShortcut, StartTimer,
                                         CompatibilityType.StartStop, (int) TimerHotkeyState.ShouldStart, NextTimerState);
            hotkeyManager.RegisterHotkey(Config.InstanceID, nameof(Config.StopShortcut), Config.StopShortcut, StopTimer,
                                         CompatibilityType.StartStop, (int) TimerHotkeyState.ShouldStop, NextTimerState);

            hotkeyManager.RegisterHotkey(Config.InstanceID, nameof(Config.ShowHideShortcut), Config.ShowHideShortcut,
                                         ShowHideTimer, CompatibilityType.Visibility);
        }

        public override bool Equals(object obj) => obj is TimerModel other && other.Config.InstanceID == Config.InstanceID;
        public override int GetHashCode() => Config.InstanceID.GetHashCode();
        public override string ToString() => Config.Name;
    }
}