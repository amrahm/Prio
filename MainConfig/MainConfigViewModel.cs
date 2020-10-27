using System;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace MainConfig {
    public class MainConfigViewModel : BindableBase, IDialogAware {
        public string Title { get; } = "Prio -- Config";
        public bool CanCloseDialog() => true;
        public void OnDialogClosed() {  }
        public void OnDialogOpened(IDialogParameters parameters) {  }
        public event Action<IDialogResult> RequestClose;
    }
}