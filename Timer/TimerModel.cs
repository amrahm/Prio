using Infrastructure.Constants;
using Infrastructure.SharedResources;
using Prism.Ioc;
using Prism.Services.Dialogs;

namespace Timer {
    public class TimerModel : ITimer {
        public TimerConfig Config { get; set; }
        private readonly IDialogService _dialogService;

        public TimerModel(TimerConfig config) {
            Config = config;
            _dialogService = UnityInstance.GetContainer().Resolve<IDialogService>();
        }

        public void SaveSettings() {
            Settings.SaveSettings(Config, ModuleNames.TIMER, Config.InstanceID);
        }

        public void ShowTimer() {
            _dialogService.Show(nameof(TimerView), new DialogParameters { { nameof(ITimer), this } }, result => { });
        }

        public void OpenSettings() {
            _dialogService.ShowDialog(nameof(TimerSettingsView), new DialogParameters {{nameof(ITimer), this}},
                                      result => { });
        }
    }
}