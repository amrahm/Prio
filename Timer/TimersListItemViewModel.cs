using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace Timer {
    public class TimersListItemViewModel : BindableBase {
        public DelegateCommand OpenTimerSettings { get; }
        public TimersListItemViewModel(IDialogService dialogService) {
            OpenTimerSettings = new DelegateCommand(() => {
                dialogService.Show(nameof(TimerSettingsView), new DialogParameters(), result => {});
            });
        }
    }
}