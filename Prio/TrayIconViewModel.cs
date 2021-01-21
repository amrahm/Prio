using System.Windows;
using Prism.Commands;
using Prism.Mvvm;
using static Infrastructure.SharedResources.UnityInstance;

namespace Prio {
    public class TrayIconViewModel : BindableBase {
        public DelegateCommand ShowWindows { get; }
        public DelegateCommand OpenMainSettings { get; }
        public DelegateCommand ExitProgram { get; }

        public TrayIconViewModel() {
            ShowWindows = new DelegateCommand(() => {
                foreach(Window window in Application.Current.Windows) window.Activate();
            });
            OpenMainSettings = new DelegateCommand(() => MainConfigService.ShowConfigWindow());
            //TODO make sure everything saves themselves before exiting 
            ExitProgram = new DelegateCommand(() => Application.Current.Shutdown());
        }
    }
}