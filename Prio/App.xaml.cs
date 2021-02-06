using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Infrastructure.Prism;
using Infrastructure.SharedResources;
using Prio.GlobalServices;
using Prio.RegionAdapters;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Prio {
    /// <summary> Interaction logic for App.xaml </summary>
    public partial class App {
        protected override void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterSingleton<IPrioHotkeyManager, PrioHotkeyManager>();
            containerRegistry.RegisterSingleton<IVirtualDesktopManager, VirtualDesktopManager>();
            containerRegistry.RegisterDialog<ColorPickerView, ColorPickerViewModel>();
            containerRegistry.RegisterDialog<MessageBoxView, MessageBoxViewModel>();
        }

        protected override Window CreateShell() {
            Current.Resources.Add(UnityInstance.CONTAINER_NAME, Container);
            FindResource("PrioTrayIcon");
            return null;
        }

        protected override IModuleCatalog CreateModuleCatalog() {
            return new DirectoryModuleCatalog {ModulePath = @"."};
        }

        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings) {
            base.ConfigureRegionAdapterMappings(regionAdapterMappings);
            regionAdapterMappings.RegisterMapping<StackPanel>(Container.Resolve<StackPanelRegionAdapter>());
            regionAdapterMappings.RegisterMapping<WrapPanel>(Container.Resolve<WrapPanelRegionAdapter>());
        }

        protected override void ConfigureDefaultRegionBehaviors(IRegionBehaviorFactory regionBehaviors) {
            base.ConfigureDefaultRegionBehaviors(regionBehaviors);
            regionBehaviors.AddIfMissing(RegionManagerAwareBehavior.BEHAVIOR_KEY, typeof(RegionManagerAwareBehavior));
        }


        #region Logging

        public App() {
            Log.Logger = new LoggerConfiguration()
                         .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 5)
                         .CreateLogger();

            if(!Debugger.IsAttached) RegisterGlobalExceptionHandling(Log.Logger);
        }

        private void RegisterGlobalExceptionHandling(ILogger log) {
            // this is the line you really want 
            AppDomain.CurrentDomain.UnhandledException += (_, args) => LogException(log, args.ExceptionObject as Exception);
            Dispatcher.UnhandledException += (_, args) => LogException(log, args.Exception);
            Current.DispatcherUnhandledException += (_, args) => LogException(log, args.Exception);
            TaskScheduler.UnobservedTaskException += (_, args) => {
                LogException(log, args.Exception);
                args.SetObserved();
            };
        }

        private static void LogException(ILogger log, Exception exception) {
            string exceptionMessage = exception?.Message ?? "An unmanaged exception occured.";
            log.Error(exception, exceptionMessage);
            MessageBox.Show(exceptionMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion
    }
}