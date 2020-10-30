using System;
using System.Diagnostics;
using System.Windows;
using Infrastructure.SharedResources;
using MainConfig;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Timer.Annotations;

namespace Timer {
    public class TimerViewModel : BindableBase, IDialogAware {
        public string Title { get; set; } = "Timer";
        public event Action<IDialogResult> RequestClose;
        [CanBeNull] public ITimer Model { get; private set; }

        [UsedImplicitly]
        public TimerConfig Config {
            get => Model?.Config;
            set {
                if(Model != null) Model.Config = value;
            }
        }

        //TODO enable/disable for hours, minutes, seconds
        [UsedImplicitly] public string TimeLeft => Config.TimeLeft.ToString(@"hh\:mm\:ss");

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
    }
}