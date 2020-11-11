﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Infrastructure.SharedResources;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace Timer {
    public class TimerSettingsViewModel : NotifyPropertyWithDependencies, IDialogAware {
        private ITimer Model { get; set; }
        private TimerConfig _config;
        public string Title { get; } = "Timer Settings";
        public event Action<IDialogResult> RequestClose;

        public TimerConfig Config {
            get => _config;
            set => NotificationBubbler.BubbleSetter(ref _config, value, (o, e) => this.OnPropertyChanged());
        }

        [DependsOnProperty(nameof(Config))]
        public int Hours {
            get => (int) (Config?.Duration.TotalHours ?? 0);
            set {
                Config.Duration = new TimeSpan(value, Minutes, Seconds);
                Config.TimeLeft = Config.Duration;
                if(Math.Abs(Config.Duration.TotalHours) < 0.0001) Minutes = 1;
            }
        }

        [DependsOnProperty(nameof(Config))]
        public int Minutes {
            get => Config?.Duration.Minutes ?? 0;
            set {
                Config.Duration = new TimeSpan(Hours, value, Seconds);
                Config.TimeLeft = Config.Duration;
            }
        }

        [DependsOnProperty(nameof(Config))]
        public int Seconds {
            get => Config?.Duration.Seconds ?? 0;
            set {
                Config.Duration = new TimeSpan(Hours, Minutes, value);
                Config.TimeLeft = Config.Duration;
            }
        }

        private static HashSet<int> StringToSet(string listString) =>
            Array.ConvertAll(listString.Trim().Trim(',').Replace(" ", "").Split(','), int.Parse).ToHashSet();

        public string ShowDesktopsConverter {
            get => string.Join(", ", Config?.DesktopsVisible ?? new HashSet<int>());
            set => Config.DesktopsVisible = StringToSet(value);
        }

        public string ActiveDesktopsConverter {
            get => string.Join(", ", Config?.DesktopsActive ?? new HashSet<int>());
            set => Config.DesktopsActive = StringToSet(value);
        }

        public DelegateCommand CancelCommand { get; }
        public DelegateCommand ApplyCommand { get; }
        public DelegateCommand OkCommand { get; }

        public TimerSettingsViewModel() {
            CancelCommand = new DelegateCommand(() => RequestClose?.Invoke(null));
            ApplyCommand = new DelegateCommand(() => {
                Model.Config = Config.DeepCopy();
                Model.SaveSettings();
            });
            OkCommand = new DelegateCommand(() => {
                Model.Config = Config.DeepCopy();
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