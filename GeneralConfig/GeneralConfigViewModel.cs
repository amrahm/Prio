using Prism.Commands;
using Prism.Mvvm;
using static Infrastructure.SharedResources.UnityInstance;

namespace GeneralConfig {
    class GeneralConfigViewModel : BindableBase {
        public DelegateCommand CancelCommand { get; }
        public DelegateCommand ApplyCommand { get; }
        public DelegateCommand OkCommand { get; }

        public GeneralConfigViewModel() {
            CancelCommand = new DelegateCommand(() => MainConfigService.CloseConfigWindow());
            ApplyCommand = new DelegateCommand(() => MainConfigService.InvokeApplySettings());
            OkCommand = new DelegateCommand(() => {
                MainConfigService.InvokeApplySettings();
                MainConfigService.CloseConfigWindow();
            });
        }
    }
}