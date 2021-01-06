using System;
using System.Windows;
using WindowsDesktop;
using JetBrains.Annotations;
using WeakEvent;

namespace Prio.GlobalServices {
    [UsedImplicitly]
    internal class VirtualDesktopManager : IVirtualDesktopManager {
        private readonly WeakEventSource<DesktopChangedEventArgs> _desktopChanged =
                new();

        public event EventHandler<DesktopChangedEventArgs> DesktopChanged {
            add => _desktopChanged.Subscribe(value);
            remove => _desktopChanged.Unsubscribe(value);
        }

        public VirtualDesktopManager() {
            VirtualDesktopProvider.Default.Initialize().Wait();
            VirtualDesktop.CurrentChanged += (o,  e) =>
                    _desktopChanged.Raise(o, new DesktopChangedEventArgs(e.OldDesktop.Index(), e.NewDesktop.Index()));
        }

        public void MoveToDesktop(Window window, int desktopNum) =>
                window?.Dispatcher.Invoke(() => window.MoveToDesktop(VirtualDesktop.GetDesktops()[desktopNum]));

        public int CurrentDesktop() => VirtualDesktop.Current.Index();
    }

    public static class VirtualDesktopExtention {
        public static int Index(this VirtualDesktop desktop) => Array.IndexOf(VirtualDesktop.GetDesktops(), desktop);
    }
}