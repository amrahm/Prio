using System;
using System.Windows.Input;
using Infrastructure.SharedResources;
using Prism.Ioc;
using MainConfig;

namespace Prio {
    public class OpenShell : ICommand {
        private readonly IContainerProvider _container;

        public OpenShell() {
            _container = UnityInstance.GetContainer();
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) {
            _container.Resolve<IMainConfigService>().ShowConfigWindow();
        }

        public event EventHandler CanExecuteChanged;
    }
}