﻿using Infrastructure.SharedResources;
using MainConfig;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;

namespace GeneralConfig {
    class GeneralConfigViewModel : BindableBase {
        public DelegateCommand CancelCommand { get; }
        public DelegateCommand ApplyCommand { get; }
        public DelegateCommand OkCommand { get; }

        public GeneralConfigViewModel() {
            IContainerProvider container = UnityInstance.GetContainer();
            var mainConfigService = container.Resolve<IMainConfigService>();
            CancelCommand = new DelegateCommand(() => mainConfigService.CloseConfigWindow());
            ApplyCommand = new DelegateCommand(() => mainConfigService.InvokeApplySettings());
            OkCommand = new DelegateCommand(() => {
                mainConfigService.InvokeApplySettings();
                mainConfigService.CloseConfigWindow();
            });
        }
    }
}