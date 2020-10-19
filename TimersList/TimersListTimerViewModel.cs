using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using TimerSettings;

namespace TimersList {
    public class TimersListTimerViewModel : BindableBase {
        public DelegateCommand OpenTimerSettings { get; }
        public TimersListTimerViewModel(IDialogService dialogService) {
            OpenTimerSettings = new DelegateCommand(() => {
                dialogService.Show(nameof(TimerSettingsView), new DialogParameters(), result => {});
            });
        }
    }
}