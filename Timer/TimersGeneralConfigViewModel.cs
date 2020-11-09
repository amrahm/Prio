using Infrastructure.SharedResources;
using MainConfig;
using Prism.Ioc;

namespace Timer {
    public class TimersGeneralConfigViewModel : NotifyPropertyChanged {
        private TimersGeneralConfig _config;

        public TimersGeneralConfig GeneralConfig {
            get => _config;
            private set => NotificationBubbler.BubbleSetter(ref _config, value, (o, e) => OnPropertyChanged());
        }

        public TimersGeneralConfigViewModel() {
            GeneralConfig = TimersService.Singleton.GeneralConfig.DeepCopy(); //TODO have to apply it at some point
            IContainerProvider container = UnityInstance.GetContainer();
            var mainConfigService = container.Resolve<IMainConfigService>();
            mainConfigService.ApplySettingsPress += SaveSettings;
        }

        public void SaveSettings() {
            TimersService.Singleton.GeneralConfig = GeneralConfig;
            TimersService.Singleton.SaveSettings();
        }
    }
}