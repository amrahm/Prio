using static Infrastructure.SharedResources.BindingHelpers;

namespace Timer {
    /// <summary> Interaction logic for TimersGeneralConfigView.xaml </summary>
    public partial class TimersGeneralConfigView  {
        public TimersGeneralConfigView() {
            InitializeComponent();

            var vm = (TimersGeneralConfigViewModel) DataContext;

            Loaded += (_,  _) => {
                void Callback() => TimersService.Singleton.RegisterShortcuts(vm.GeneralConfig);
                ManualBinding(vm.GeneralConfig, nameof(vm.GeneralConfig.ToggleVisibilityShortcut), ToggleVisibilityShortcut,
                              nameof(ToggleVisibilityShortcut.Shortcut), callback: Callback);
                ManualBinding(vm.GeneralConfig, nameof(vm.GeneralConfig.KeepTimersOnTopShortcut), KeepOnTopShortcut,
                              nameof(KeepOnTopShortcut.Shortcut), callback: Callback);
                ManualBinding(vm.GeneralConfig, nameof(vm.GeneralConfig.MoveTimersBehindShortcut), MoveBehindShortcut,
                              nameof(MoveBehindShortcut.Shortcut), callback: Callback);

                ManualBinding(vm.GeneralConfig, nameof(vm.GeneralConfig.StopAllShortcut), StopAllShortcut,
                              nameof(StopAllShortcut.Shortcut), callback: Callback);
                ManualBinding(vm.GeneralConfig, nameof(vm.GeneralConfig.ResumeAllShortcut), ResumeAllShortcut,
                              nameof(ResumeAllShortcut.Shortcut), callback: Callback);
            };
        }
    }
}