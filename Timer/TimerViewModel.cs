using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace Timer {
    public class TimerViewModel : BindableBase, ITimer {
        public string Title { get; set; } = "Timer";
        private readonly IDialogService _dialogService;

        public TimerViewModel(IDialogService dialogService) {
            _dialogService = dialogService;
        }

        public void OpenSettings() {
            _dialogService.ShowDialog(nameof(TimerSettingsView), new DialogParameters(), result => { });
        }
    }
}