using System.Windows;
using Prism.Ioc;
using Prism.Modularity;
using TimerSettings;

namespace Prio {
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    internal partial class App {
        protected override void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterDialog<TimerSettingsView, TimerSettingsViewModel>();
        }

        protected override Window CreateShell() {
            return Container.Resolve<ShellView>();
        }

        protected override IModuleCatalog CreateModuleCatalog() {
            return new DirectoryModuleCatalog {ModulePath = @"."};
        }
    }
}