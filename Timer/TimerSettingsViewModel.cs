using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Prism.Commands;
using Prism.Services.Dialogs;
using Infrastructure.Constants;
using Infrastructure.SharedResources;
using Prism.Mvvm;
using Timer.Annotations;

namespace Timer {
    public class TimerSettingsViewModel : BindableBase, IDialogAware {
        private readonly TimerModel _model;
        private int _hours = 1;
        private int _minutes;
        private int _seconds;
        public string Title { get; } = "Timer Settings";
        public event Action<IDialogResult> RequestClose;

        public TimerConfig Config {
            get => _model.Config;
            set => _model.Config = value;
        }

        public int Hours {
            get => _hours;
            set {
                Config.Duration = new TimeSpan(value, _minutes, _seconds);
                double totalHours = Config.Duration.TotalHours;
                if(Math.Abs(totalHours) < 0.0001) {
                    Minutes = 1;
                }
                SetProperty(ref _hours, (int) totalHours); //To allow values greater than 24
                Config.Name = "OOO";
            }
        }

        public int Minutes {
            get => _minutes;
            set {
                Config.Duration = new TimeSpan(_hours, value, _seconds);
                SetProperty(ref _minutes, Config.Duration.Minutes);
                Hours = (int) Config.Duration.TotalHours;
            }
        }

        public int Seconds {
            get => _seconds;
            set {
                Config.Duration = new TimeSpan(_hours, _minutes, value);
                SetProperty(ref _seconds, Config.Duration.Seconds);
                int durationHours = (int) Config.Duration.TotalHours; //cache this before minutes calls OnPropertyChanged
                Minutes = Config.Duration.Minutes;
                Hours = durationHours;
            }
        }

        public void HandleKeyDown(KeyEventArgs e) { /* ... */ }

        public DelegateCommand CancelCommand { get; }
        public DelegateCommand ApplyCommand { get; }
        public DelegateCommand OkCommand { get; }

        public TimerSettingsViewModel() {
            CancelCommand = new DelegateCommand(() => RequestClose?.Invoke(null));
            ApplyCommand = new DelegateCommand(() => Settings.SaveSettings(Config, ModuleNames.TIMER, Config.InstanceID));
            OkCommand = new DelegateCommand(() => {
                Settings.SaveSettings(Config, ModuleNames.TIMER, Config.InstanceID);
                RequestClose?.Invoke(null);
            });

            //TODO how to load config
            _model = new TimerModel(new TimerConfig(), this);
        }

        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters) {
            //TODO load config from parameters
            Config = new TimerConfig();
        }
    }
}