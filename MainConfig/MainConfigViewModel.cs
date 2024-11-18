using Infrastructure.SharedResources;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;

namespace MainConfig {
    public class MainConfigViewModel : BindableBase, IDialogAware {
        public string Title { get; } = "Prio -- Config";
        public bool CanCloseDialog() => true;
        public void OnDialogClosed() {  }
        public void OnDialogOpened(IDialogParameters parameters) {  }

        DialogCloseListener IDialogAware.RequestClose { get; }


        public DelegateCommand ExitCommand { get; }

        public MainConfigViewModel() {
            ExitCommand = new DelegateCommand(() => UnityInstance.MainConfigService.CloseConfigWindow());
        }
    }
}
