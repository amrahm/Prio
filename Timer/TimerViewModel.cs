using System;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace Timer {
    internal class TimerViewModel : BindableBase, IDialogAware {
        public string Title { get; set; } = "Timer";
        public event Action<IDialogResult> RequestClose;

        public TimerViewModel() {
            CloseCommand = new DelegateCommand(() => RequestClose?.Invoke(null));
        }

        public DelegateCommand CloseCommand { get; }


        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }
        public void OnDialogOpened(IDialogParameters parameters) { }
    }
}