using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Dialogs;
using DialogResult = Prism.Dialogs.DialogResult;

namespace Infrastructure.SharedResources {
    public class MessageBoxViewModel : NotifyPropertyChanged, IDialogAware {
        public DialogCloseListener RequestClose { get; }
        public string Title { get; set; }
        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }

        public DelegateCommand OkCommand { get; }
        public DelegateCommand CancelCommand { get; }

        public string Message { get; set; }
        public string OkText { get; private set; } = "OK";
        internal bool getsFocus = true;
        internal Screen openOnScreen;
        public bool HasCancel { get; set; }
        public string CancelText { get; private set; } = "Cancel";

        internal bool isCountdown;
        internal string customOk = "OK";
        private readonly DispatcherTimer _timer = new() {Interval = TimeSpan.FromSeconds(1)};
        private int _secondsLeft = 5;

        public MessageBoxViewModel() {
            _timer.Tick += (_,  _) => {
                _secondsLeft--;
                OkText = $"OK ({_secondsLeft})";
                if(_secondsLeft <= 0) RequestClose.Invoke(new DialogResult(ButtonResult.OK));
            };

            OkCommand = new DelegateCommand(() => RequestClose.Invoke(new DialogResult(ButtonResult.OK)));
            CancelCommand = new DelegateCommand(() => RequestClose.Invoke(new DialogResult(ButtonResult.Cancel)));
        }

        public void OnDialogOpened(IDialogParameters parameters) {
            Message = parameters.GetValue<string>(nameof(Message));
            Title = parameters.GetValue<string>(nameof(Title));
            isCountdown = parameters.GetValue<bool>(nameof(isCountdown));
            getsFocus = parameters.GetValue<bool>(nameof(getsFocus));
            openOnScreen = parameters.GetValue<Screen>(nameof(openOnScreen));
            HasCancel = parameters.GetValue<bool>(nameof(HasCancel));
            OkText = customOk = parameters.GetValue<string>(nameof(customOk)) ?? customOk;
            CancelText = parameters.GetValue<string>(nameof(CancelText)) ?? CancelText;
            if(isCountdown) {
                OkText = $"{customOk} ({_secondsLeft})";
                _timer.Start();
            }
        }
    }

    public static class DialogServiceMessageExtension {
        public static Task<IDialogResult> ShowNotification(this IDialogService dialogService, string message, string title,
                                                           bool isCountdown = false, bool getsFocus = true,
                                                           bool modal = true, Screen openOnScreen = null,
                                                           bool hasCancel = false, string customOk = null,
                                                           string customCancel = null) {
            DialogParameters dialogParameters = new()  {
                {nameof(MessageBoxViewModel.Message), message},
                {nameof(MessageBoxViewModel.Title), title},
                {nameof(MessageBoxViewModel.isCountdown), isCountdown},
                {nameof(MessageBoxViewModel.getsFocus), getsFocus},
                {nameof(MessageBoxViewModel.openOnScreen), openOnScreen},
                {nameof(MessageBoxViewModel.HasCancel), hasCancel},
                {nameof(MessageBoxViewModel.customOk), customOk},
                {nameof(MessageBoxViewModel.CancelText), customCancel}
            };
            return modal ?
                       dialogService.ShowDialogAsync(nameof(MessageBoxView), dialogParameters) :
                       dialogService.ShowAsync(nameof(MessageBoxView), dialogParameters);
        }
    }
}
