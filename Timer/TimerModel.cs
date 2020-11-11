using System;
using System.Windows;
using System.Windows.Interop;
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

        public Window OpenWindow { get; set; }

        private readonly IDialogService _dialogService;
        private readonly DispatcherTimer _timer;
        public bool IsRunning => _timer.IsEnabled;

        public TimerModel(TimerConfig config) {
            Config = config;
            IContainerProvider container = UnityInstance.GetContainer();
            _dialogService = container.Resolve<IDialogService>();

            _timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            _timer.Tick += (o,  e) => Config.TimeLeft -= TimeSpan.FromSeconds(1);

            RegisterShortcuts();

            //VirtualDesktop[] desktops = VirtualDesktop.GetDesktops();
            //var desktopMap = new Dictionary<Guid, int>();
            //for (int i = 0; i < desktops.Length; i++) desktopMap.Add(desktops[i].Id, i);
            //VirtualDesktop.CurrentChanged += (o, e) => {
            //    OpenWindow?.Dispatcher.Invoke(() => {
            //        //Debug.WriteLine($"Desktop changed: {e.NewDesktop.Id}");
            //        if (Config.DesktopsVisible.Contains(desktopMap[e.NewDesktop.Id])) OpenWindow.MoveToDesktop(e.NewDesktop);
            //        //OpenWindow.MoveToDesktop(VirtualDesktop.Current);
            //    });
            //};

            //var vdm = new VirtualDesktopManager();
            //var timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(0.1)};
            //timer.Tick += (o,  e) => {
            //    if(OpenWindow != null) {
            //        var hWnd = new WindowInteropHelper(OpenWindow).Handle;
            //        if(!vdm.IsWindowOnCurrentVirtualDesktop(hWnd)) {
            //            vdm.MoveWindowToDesktop(hWnd, vdm.GetWindowDesktopId(hWnd));
            //        }
            //    }
            //};
            //timer.Start();
        }


        public void ShowTimer() {
            _dialogService.Show(nameof(TimerView), new DialogParameters {{nameof(ITimer), this}}, result => { });
            TimersService.Singleton.ApplyVisState();
        }

        public void ResetTimer() => Config.TimeLeft = Config.Duration;
        public void StartTimer() => _timer.Start();
        public void StopTimer() => _timer.Stop();
        private event Action RequestHide;

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

        public void OpenSettings() {
            StopTimer();
            _dialogService.ShowDialog(nameof(TimerSettingsView), new DialogParameters {{nameof(ITimer), this}},
                                      result => { });
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
                                         RequestHide, CompatibilityType.Visibility);
        }
    }
}