using System;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace TimerSettings {
    public class TimerSettingsViewModel : BindableBase, IDialogAware {
        public string Title { get; } = "Timer Settings";
        public event Action<IDialogResult> RequestClose;

        public TimerSettingsViewModel() {
            CloseCommand = new DelegateCommand(() => RequestClose(null));
        }

        public DelegateCommand CloseCommand { get; }


        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }
        public void OnDialogOpened(IDialogParameters parameters) { }

        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }
    }
}