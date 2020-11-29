﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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

        public Window TimerWindow { get; set; }

        private readonly IDialogService _dialogService;
        private readonly DispatcherTimer _timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
        private bool _hidden;
        private readonly IVirtualDesktopManager _vdm;

        public bool IsRunning => _timer.IsEnabled;

        public TimerModel(TimerConfig config) {
            Config = config;
            IContainerProvider container = UnityInstance.GetContainer();
            _dialogService = container.Resolve<IDialogService>();

            _timer.Tick += (o,  e) => Config.TimeLeft -= TimeSpan.FromSeconds(1);

            RegisterShortcuts();

            _vdm = container.Resolve<IVirtualDesktopManager>();
            _vdm.DesktopChanged += (o, e) => HandleDesktopChanged(e.NewDesktop);
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

        public void ResetTimer() => Config.TimeLeft = Config.Duration;
        public void StartTimer() => _timer.Start();
        public void StopTimer() => _timer.Stop();

        public void ShowHideTimer() {
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
            hotkeyManager.RegisterHotkey(Config.InstanceID, nameof(Config.ResetShortcut), Config.ResetShortcut, ResetTimer,
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