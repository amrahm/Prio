using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Prism.Commands;
using Prism.Services.Dialogs;
using Infrastructure.Constants;
using Infrastructure.SharedResources;
using Timer.Annotations;

namespace Timer {
    public class TimerSettingsViewModel : INotifyPropertyChanged, IDialogAware {
        private TimerConfig _config;
        public string Title { get; } = "Timer Settings";
        public event Action<IDialogResult> RequestClose;

        public TimerConfig Config {
            get => _config;
            set {
                _config = value;
                OnPropertyChanged();
            }
        }


        public TimerSettingsViewModel() {
            CancelCommand = new DelegateCommand(() => RequestClose?.Invoke(null));
            ApplyCommand = new DelegateCommand(() => Settings.SaveSettings(Config, ModuleNames.TIMER));
        }

        public DelegateCommand CancelCommand { get; }
        public DelegateCommand ApplyCommand { get; }


        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters) {
            //TODO load config from parameters
            Config = new TimerConfig();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}