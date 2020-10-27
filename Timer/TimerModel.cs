using Infrastructure.Constants;
using Infrastructure.SharedResources;

namespace Timer {
    public class TimerModel {
        public TimerConfig Config { get; set; }
        private readonly TimerSettingsViewModel _vm;
        public TimerModel(TimerConfig config, TimerSettingsViewModel vm) {
            Config = config;
            _vm = vm;
        }

        public void SaveSettings() {
            Settings.SaveSettings(Config, ModuleNames.TIMER, Config.InstanceID);
        }
    }
}