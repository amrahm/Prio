using Infrastructure.Constants;
using Infrastructure.SharedResources;
using Prism.Ioc;

namespace Timer {
    public class TimerModel : ITimer {
        public TimerConfig Config { get; set; }
        private readonly TimerViewModel _vm;
        private IContainerProvider _container;

        public TimerModel(TimerConfig config, TimerViewModel vm) {
            Config = config;
            _vm = vm;
            _container = UnityInstance.GetContainer();
        }

        public void SaveSettings() {
            Settings.SaveSettings(Config, ModuleNames.TIMER, Config.InstanceID);
        }

        public void OpenSettings() {
            _vm.OpenSettings();
        }
    }
}