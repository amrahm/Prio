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
        }


        public void ShowTimer() =>
            _dialogService.Show(nameof(TimerView), new DialogParameters {{nameof(ITimer), this}}, result => { });

        public void ResetTimer() => Config.TimeLeft = Config.Duration;
        public void StartTimer() => _timer.Start();
        public void StopTimer() => _timer.Stop();

        public void OpenSettings() {
            StopTimer();
            _dialogService.ShowDialog(nameof(TimerSettingsView), new DialogParameters {{nameof(ITimer), this}},
                                      result => { });
        }

        public void SaveSettings() {
            Settings.SaveSettings(Config, ModuleNames.TIMER, Config.InstanceID);
            RegisterShortcuts();
        }

        public void RegisterShortcuts() {
            IContainerProvider container = UnityInstance.GetContainer();
            var hotkeyManager = container.Resolve<IPrioHotkeyManager>();
            hotkeyManager.RegisterHotkey(Config.InstanceID, nameof(Config.ResetShortcut), Config.ResetShortcut, ResetTimer);
            hotkeyManager.RegisterHotkey(Config.InstanceID, nameof(Config.StartShortcut), Config.StartShortcut, StartTimer);
            hotkeyManager.RegisterHotkey(Config.InstanceID, nameof(Config.StopShortcut), Config.StopShortcut, StopTimer);
        }
    }
}