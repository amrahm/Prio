using System;
using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<VisibilityState> VisibilityStateTypeValues =>
            Enum.GetValues(typeof(VisibilityState)).Cast<VisibilityState>();

        public TimersGeneralConfigViewModel() {
            GeneralConfig = TimersService.Singleton.GeneralConfig.DeepCopy();
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