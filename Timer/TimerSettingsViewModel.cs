using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Prism.Commands;
using Prism.Services.Dialogs;
using Infrastructure.Constants;
using Infrastructure.SharedResources;
using Timer.Annotations;

namespace Timer {
    public class TimerSettingsViewModel : INotifyPropertyChanged, IDialogAware {
        private TimerConfig _config;
        private int _hours = 1;
        private int _minutes;
        private int _seconds;
        public string Title { get; } = "Timer Settings";
        public event Action<IDialogResult> RequestClose;

        public TimerConfig Config {
            get => _config;
            set {
                _config = value;
                OnPropertyChanged();
            }
        }

        public int Hours {
            get => _hours;
            set {
                Config.Duration = new TimeSpan(value, _minutes, _seconds);
                double totalHours = Config.Duration.TotalHours;
                if(Math.Abs(totalHours) < 0.0001) {
                    Minutes = 1;
                }
                _hours = (int) totalHours; //To allow values greater than 24
                OnPropertyChanged();
            }
        }

        public int Minutes {
            get => _minutes;
            set {
                Config.Duration = new TimeSpan(_hours, value, _seconds);
                _minutes = Config.Duration.Minutes;
                Hours = (int) Config.Duration.TotalHours;
                OnPropertyChanged();
            }
        }

        public int Seconds {
            get => _seconds;
            set {
                Config.Duration = new TimeSpan(_hours, _minutes, value);
                _seconds = Config.Duration.Seconds;
                int durationHours = (int) Config.Duration.TotalHours; //cache this before minutes calls OnPropertyChanged
                Minutes = Config.Duration.Minutes;
                Hours = durationHours;
                OnPropertyChanged();
            }
        }

        public void HandleKeyDown(KeyEventArgs e) { /* ... */ }

        public DelegateCommand CancelCommand { get; }
        public DelegateCommand ApplyCommand { get; }
        public DelegateCommand OkCommand { get; }

        public TimerSettingsViewModel() {
            CancelCommand = new DelegateCommand(() => RequestClose?.Invoke(null));
            ApplyCommand = new DelegateCommand(() => Settings.SaveSettings(Config, ModuleNames.TIMER));
            OkCommand = new DelegateCommand(() => {
                Settings.SaveSettings(Config, ModuleNames.TIMER);
                RequestClose?.Invoke(null);
            });
        }

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