using System;
using System.Windows.Input;
using Infrastructure.SharedResources;
using Prism.Ioc;
using Prism.Services.Dialogs;
using MainConfig;

namespace Prio {
    public class OpenShell : ICommand {
        private IDialogService _dialogService;

        public OpenShell() {
            _dialogService = UnityInstance.GetContainer().Resolve<IDialogService>();
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) {
            _dialogService.Show(nameof(MainConfigView), new DialogParameters(), result => { });
        }

        public event EventHandler CanExecuteChanged;
    }
}