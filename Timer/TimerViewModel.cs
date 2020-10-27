using System;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Timer.Annotations;

namespace Timer {
    public class TimerViewModel : BindableBase, IDialogAware {
        public string Title { get; set; } = "Timer";
        public event Action<IDialogResult> RequestClose;
        private readonly IDialogService _dialogService;
        [CanBeNull] private TimerModel _model;

        public TimerConfig Config {
            get => _model?.Config;
            set {
                if(_model != null) _model.Config = value;
            }
        }

        public TimerViewModel(IDialogService dialogService) {
            _dialogService = dialogService;
        }

        public void OnDialogOpened(IDialogParameters parameters) {
            TimerConfig config = parameters.GetValue<TimerConfig>(nameof(TimerConfig));
            _model = new TimerModel(config, this);
        }

        public void OpenSettings() {
            _dialogService.ShowDialog(nameof(TimerSettingsView), new DialogParameters {{nameof(TimerModel), _model}},
                                      result => { });
        }

        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }
    }
}