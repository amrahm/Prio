using System.Windows;
using MainConfig;
using Prio.GlobalServices;
using Prism.Dialogs;
using Prism.Ioc;

namespace Infrastructure.SharedResources {
    public static class UnityInstance {
        public const string CONTAINER_NAME = nameof(CONTAINER_NAME);

        public static readonly IContainerProvider Container =
            (IContainerProvider) Application.Current.Resources[CONTAINER_NAME];

        public static readonly IDialogService Dialogs = Container.Resolve<IDialogService>();
        public static readonly IVirtualDesktopManager VDM = Container.Resolve<IVirtualDesktopManager>();
        public static readonly IPrioHotkeyManager HotkeyManager = Container.Resolve<IPrioHotkeyManager>();
        public static readonly IMainConfigService MainConfigService = Container.Resolve<IMainConfigService>();
    }
}
