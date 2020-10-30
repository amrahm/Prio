using System;
using System.Windows.Threading;
using Infrastructure.Constants;
using Infrastructure.SharedResources;
using Prism.Ioc;
using Prism.Services.Dialogs;

namespace Timer {
    public class TimerModel : ITimer {
        public TimerConfig Config { get; set; }

        private readonly IDialogService _dialogService;
        private readonly DispatcherTimer _timer;
        public bool IsRunning => _timer.IsEnabled;

        public TimerModel(TimerConfig config) {
            Config = config;
            _dialogService = UnityInstance.GetContainer().Resolve<IDialogService>();

            _timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
            _timer.Tick += (o,  e) => {
                Config.TimeLeft -= TimeSpan.FromSeconds(1);
            };
        }


        public void ShowTimer() =>
            _dialogService.Show(nameof(TimerView), new DialogParameters {{nameof(ITimer), this}}, result => { });

        public void StartTimer() => _timer.Start();

        public void StopTimer() => _timer.Stop();

        public void OpenSettings() {
            StopTimer();
            _dialogService.ShowDialog(nameof(TimerSettingsView), new DialogParameters {{nameof(ITimer), this}},
                                      result => { });
        }

        public void SaveSettings() {
            Settings.SaveSettings(Config, ModuleNames.TIMER, Config.InstanceID);
        }
    }
}