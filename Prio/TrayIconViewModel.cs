using System.Windows;
using Infrastructure.SharedResources;
using MainConfig;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;

namespace Prio {
    public class TrayIconViewModel : BindableBase {
        public DelegateCommand ShowWindows { get; }
        public DelegateCommand OpenMainSettings { get; }
        public DelegateCommand ExitProgram { get; }

        public TrayIconViewModel() {
            var container = UnityInstance.Container;
            ShowWindows = new DelegateCommand(() => {
                foreach(Window window in Application.Current.Windows) window.Activate();
            });
            OpenMainSettings = new DelegateCommand(() => container.Resolve<IMainConfigService>().ShowConfigWindow());
            //TODO make sure everything saves themselves before exiting 
            ExitProgram = new DelegateCommand(() => Application.Current.Shutdown());
        }
    }
}