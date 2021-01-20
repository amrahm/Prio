using System;
using Infrastructure.SharedResources;
using Prism.Commands;
using Prism.Services.Dialogs;
using DialogResult = Prism.Services.Dialogs.DialogResult;

namespace Timer {
    public class ChangeTimeViewModel : NotifyPropertyChanged, IDialogAware {
        public string Title { get; set; }
        public event Action<IDialogResult> RequestClose;
        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }

        public DelegateCommand OkCommand { get; }
        public DelegateCommand CancelCommand { get; }

        public TimeSpan Duration { get; set; }

        public ChangeTimeViewModel() {
            OkCommand = new DelegateCommand(
                () => RequestClose?.Invoke(new DialogResult(ButtonResult.OK,
                                                            new DialogParameters {{nameof(Duration), Duration}})));
            CancelCommand = new DelegateCommand(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));
        }

        public void OnDialogOpened(IDialogParameters parameters) {
            Title = $"Set Time of <italic>{parameters.GetValue<string>(nameof(Title))}</italic>";
            Duration = parameters.GetValue<TimeSpan>(nameof(Duration));
        }
    }
}