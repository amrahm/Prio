using System;
using Infrastructure.SharedResources;
using JetBrains.Annotations;
using Prism.Commands;
using Prism.Dialogs;

namespace Timer {
    public class ChangeTimeViewModel : NotifyPropertyChanged, IDialogAware {
        public string Title { get; set; }
        [UsedImplicitly] public DialogCloseListener RequestClose { get; }
        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }

        public DelegateCommand OkCommand { get; }
        public DelegateCommand CancelCommand { get; }

        public TimeSpan Duration { get; set; }

        public ChangeTimeViewModel() {
            OkCommand = new DelegateCommand(
                () => RequestClose.Invoke(new DialogParameters {{nameof(Duration), Duration}}, ButtonResult.OK));
            CancelCommand = new DelegateCommand(() => RequestClose.Invoke(ButtonResult.Cancel));
        }

        public void OnDialogOpened(IDialogParameters parameters) {
            Title = $"Set Time of <italic>{parameters.GetValue<string>(nameof(Title))}</italic>";
            Duration = parameters.GetValue<TimeSpan>(nameof(Duration));
        }
    }
}
