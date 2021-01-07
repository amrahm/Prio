using System;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace Infrastructure.SharedResources {
    public class MessageBoxViewModel : NotifyPropertyChanged, IDialogAware {
        public string Title { get; set; }
        public event Action<IDialogResult> RequestClose;
        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public DelegateCommand OkCommand { get; }
        public DelegateCommand CancelCommand { get; }

        public string Message { get; set; }
        public string OkText { get; private set; } = "OK";
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
                if(_secondsLeft <= 0) RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
            };

            OkCommand = new DelegateCommand(() => RequestClose?.Invoke(new DialogResult(ButtonResult.OK)));
            CancelCommand = new DelegateCommand(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));
        }

        public void OnDialogOpened(IDialogParameters parameters) {
            Message = parameters.GetValue<string>(nameof(Message));
            Title = parameters.GetValue<string>(nameof(Title));
            isCountdown = parameters.GetValue<bool>(nameof(isCountdown));
            HasCancel = parameters.GetValue<bool>(nameof(HasCancel));
            OkText = customOk = parameters.GetValue<string>(nameof(customOk)) ?? customOk;
            CancelText = parameters.GetValue<string>(nameof(CancelText)) ?? CancelText;
            if(isCountdown) {
                OkText = $"{customOk} ({_secondsLeft})";
                _timer.Start();
            }
        }
    }
}