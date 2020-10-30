using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using Infrastructure.SharedResources;
using MainConfig;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Timer.Annotations;

namespace Timer {
    public class TimerViewModel : INotifyPropertyChanged, IDialogAware {
        public string Title { get; set; } = "Timer";
        public event Action<IDialogResult> RequestClose;

        [CanBeNull] private ITimer _model;

        public ITimer Model {
            get => _model;
            set { // Bubble up changes from within
                if(_model != value) {
                    // Clean-up old event handler:
                    if(_model != null) _model.PropertyChanged -= ThisChanged;

                    _model = value;

                    if(_model != null) _model.PropertyChanged += ThisChanged;
                }

                void ThisChanged(object sender, PropertyChangedEventArgs args) {
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TimeLeftVm));
                }
            }
        }

        //TODO enable/disable for hours, minutes, seconds
        [UsedImplicitly] public string TimeLeftVm => Model.Config.TimeLeft.ToString(@"hh\:mm\:ss");

        public DelegateCommand OpenTimerSettings { [UsedImplicitly] get; }
        public DelegateCommand OpenMainSettings { [UsedImplicitly] get; }
        public DelegateCommand StartStopTimer { [UsedImplicitly] get; }
        public DelegateCommand ExitProgram { [UsedImplicitly] get; }

        public TimerViewModel(ITimer timerModel) {
            Model = timerModel;
            IContainerProvider container = UnityInstance.GetContainer();
            OpenTimerSettings = new DelegateCommand(() => Model?.OpenSettings());
            OpenMainSettings = new DelegateCommand(() => container.Resolve<IMainConfigService>().ShowConfigWindow());
            StartStopTimer = new DelegateCommand(() => {
                Debug.Assert(Model != null, nameof(Model) + " != null");
                if(Model.IsRunning) Model.StopTimer();
                else Model.StartTimer();
            });
            ExitProgram = new DelegateCommand(() => Application.Current.Shutdown());
        }

        public void OnDialogOpened(IDialogParameters parameters) {
            Model = parameters.GetValue<ITimer>(nameof(ITimer));
        }

        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}