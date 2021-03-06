﻿using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.SharedResources;
using MainConfig;
using Prism.Ioc;

namespace Timer {
    public class TimersGeneralConfigViewModel : NotifyPropertyWithDependencies {
        private readonly TimersGeneralConfig _config;

        public TimersGeneralConfig GeneralConfig {
            get => _config;
            private init => NotificationBubbler.BubbleSetter(ref _config, value, (_, _) => this.OnPropertyChanged());
        }

        public IEnumerable<VisibilityState> VisibilityStateTypeValues =>
                Enum.GetValues(typeof(VisibilityState)).Cast<VisibilityState>();

        public TimersGeneralConfigViewModel() {
            GeneralConfig = TimersService.Config.DeepCopy();
            var mainConfigService = UnityInstance.Container.Resolve<IMainConfigService>();
            mainConfigService.ApplySettingsPress += SaveSettings;
        }

        private void SaveSettings() {
            TimersService.Config = GeneralConfig;
            TimersService.Singleton.SaveSettings();
            TimersService.Singleton.ApplyVisState();
        }
    }
}