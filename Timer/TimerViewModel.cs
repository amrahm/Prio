using System;
using System.Diagnostics;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Timer.Annotations;

namespace Timer {
    public class TimerViewModel : BindableBase, IDialogAware {
        public string Title { get; set; } = "Timer";
        public event Action<IDialogResult> RequestClose;
        [CanBeNull] public ITimer Model { get; private set; }

        public TimerConfig Config {
            get => Model?.Config;
            set {
                if(Model != null) Model.Config = value;
            }
        }

        public TimerViewModel(ITimer timerModel) {
            Model = timerModel;
        }

        public void OnDialogOpened(IDialogParameters parameters) {
            Model = parameters.GetValue<ITimer>(nameof(ITimer));
            Debug.Assert(Model != null, nameof(Model) + " != null");
        }

        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }
    }
}