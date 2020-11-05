using System;
using Infrastructure.SharedResources;
using Prism.Commands;
using Prism.Services.Dialogs;
using Prism.Mvvm;

namespace Timer {
    public class TimerSettingsViewModel : BindableBase, IDialogAware {
        private ITimer Model { get; set; }
        private int _hours = 1;
        private int _minutes;
        private int _seconds;
        public string Title { get; } = "Timer Settings";
        public event Action<IDialogResult> RequestClose;

        public TimerConfig Config { get; set; }

        public int Hours {
            get => _hours;
            set {
                    Config.Duration = new TimeSpan(value, _minutes, _seconds);
                    Config.TimeLeft = Config.Duration;
                    double totalHours = Config.Duration.TotalHours;
                    if(Math.Abs(totalHours) < 0.0001) {
                        Minutes = 1;
                    }
                    _hours =  (int) totalHours; //To allow values greater than 24
            }
        }

        public int Minutes {
            get => _minutes;
            set {
                Config.Duration = new TimeSpan(_hours, value, _seconds);
                _minutes = Config.Duration.Minutes;
                Hours = (int) Config.Duration.TotalHours;
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
            }
        }

        public DelegateCommand CancelCommand { get; }
        public DelegateCommand ApplyCommand { get; }
        public DelegateCommand OkCommand { get; }

        public TimerSettingsViewModel() {
            CancelCommand = new DelegateCommand(() => RequestClose?.Invoke(null));
            ApplyCommand = new DelegateCommand(() => {
                Model.Config = Config;
                Model.SaveSettings();
            });
            OkCommand = new DelegateCommand(() => {
                Model.Config = Config;
                Model.SaveSettings();
                RequestClose?.Invoke(null);
            });
        }

        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters) {
            Model = parameters.GetValue<ITimer>(nameof(ITimer));
            Config = Model.Config.DeepCopy();
        }
    }
}