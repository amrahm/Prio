using static Infrastructure.SharedResources.BindingHelpers;

namespace Timer {
    /// <summary> Interaction logic for TimersGeneralConfigView.xaml </summary>
    public partial class TimersGeneralConfigView  {
        public TimersGeneralConfigView() {
            InitializeComponent();

            var vm = (TimersGeneralConfigViewModel) DataContext;

            Loaded += (o,  e) => {
                ManualBinding(vm.GeneralConfig, nameof(vm.GeneralConfig.ShowHideTimersShortcut), ShowHideShortcut,
                              nameof(ShowHideShortcut.Shortcut));
                ManualBinding(vm.GeneralConfig, nameof(vm.GeneralConfig.KeepTimersOnTopShortcut), KeepOnTopShortcut,
                              nameof(KeepOnTopShortcut.Shortcut));
                ManualBinding(vm.GeneralConfig, nameof(vm.GeneralConfig.MoveTimersBehindShortcut), MoveBehindShortcut,
                              nameof(MoveBehindShortcut.Shortcut));
            };
        }
    }
}