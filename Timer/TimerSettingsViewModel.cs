using System;
using System.Collections.ObjectModel;
using Infrastructure.SharedResources;
using Prism.Commands;
using Prism.Services.Dialogs;
using static Infrastructure.SharedResources.VirtualDesktopExtensions;

namespace Timer {
    public class TimerSettingsViewModel : NotifyPropertyWithDependencies, IDialogAware {
        private ITimer Model { get; set; }
        public string Title { get; } = "Timer Settings";
        public event Action<IDialogResult> RequestClose;

        private TimerConfig _config;
        public TimerConfig Config {
            get => _config;
            set {
                NotificationBubbler.BubbleSetter(ref _config, value, (_, _) => this.OnPropertyChanged());

                OverflowActionViews.Clear();
                foreach(OverflowAction action in Config.OverflowActions) AddActionView(action);
            }
        }

        public ObservableCollection<OverflowActionView> OverflowActionViews { get; } = new();

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

        public string ShowDesktopsConverter {
            get => DesktopSetToString(Config?.DesktopsVisible);
            set => Config.DesktopsVisible = DesktopStringToSet(value);
        }

        public string ActiveDesktopsConverter {
            get => DesktopSetToString(Config?.DesktopsActive);
            set => Config.DesktopsActive = DesktopStringToSet(value);
        }

        private OverflowActionViewModel _zeroOverflowActionVm;
        public OverflowActionViewModel ZeroOverflowActionVm => _zeroOverflowActionVm ??=
                Config != null ? new OverflowActionViewModel(Config.ZeroOverflowAction) : null;

        private void AddAction(OverflowAction overflowAction) {
            Config.OverflowActions.Add(overflowAction);
            AddActionView(overflowAction);
        }

        private void AddActionView(OverflowAction overflowAction) {
            OverflowActionView view = new(new OverflowActionViewModel(overflowAction));
            OverflowActionViews.Add(view);
            overflowAction.DeleteRequested += (_,  _) => {
                Config.OverflowActions.Remove(overflowAction);
                OverflowActionViews.Remove(view);
            };
        }

        public DelegateCommand AddResetConditionCommand { get; }
        public DelegateCommand AddOverflowActionCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public DelegateCommand ApplyCommand { get; }
        public DelegateCommand OkCommand { get; }

        public TimerSettingsViewModel() {
            AddResetConditionCommand =
                    new DelegateCommand(() => Config.ResetConditions.AddCondition(new ResetCondition(Model)));
            AddOverflowActionCommand = new DelegateCommand(() => AddAction(new OverflowAction(Config.InstanceID)));

            CancelCommand = new DelegateCommand(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));
            ApplyCommand = new DelegateCommand(() => {
                Model.Config = Config.DeepCopy();
                Model.SaveSettings();
                TimersService.Singleton.Timers.Add(Model);
                TimersService.Singleton.SaveSettings();
                Model.ShowTimer();
            });
            OkCommand = new DelegateCommand(() => {
                Model.Config = Config.DeepCopy();
                Model.SaveSettings();
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
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