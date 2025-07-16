using System;
using System.Windows;
using WindowsDesktop;
using JetBrains.Annotations;
using WeakEvent;

namespace Prio.GlobalServices {
    [UsedImplicitly]
    internal class VirtualDesktopManager : IVirtualDesktopManager {
        private readonly WeakEventSource<VirtualDesktopChangedEventArgs> _desktopChanged = new();

        public VirtualDesktopManager() {
            VirtualDesktopProvider.Default.Initialize().Wait();
            VirtualDesktop.CurrentChanged += (o,  e) => _desktopChanged.Raise(o, e);
        }

        public event EventHandler<VirtualDesktopChangedEventArgs> DesktopChanged {
            add => _desktopChanged.Subscribe(value);
            remove => _desktopChanged.Unsubscribe(value);
        }

        public void MoveToDesktop(Window window, int desktopNum) =>
            window?.Dispatcher.Invoke(() => window.MoveToDesktop(VirtualDesktop.GetDesktops()[desktopNum]));

        public int CurrentDesktop() => VirtualDesktop.Current.Index();
        public int NumDesktops() => VirtualDesktop.GetDesktops().Length;

        public void SwitchToDesktop(int desktopNum) => VirtualDesktop.GetDesktops()[desktopNum].Switch();
    }

    public static class VirtualDesktopExtention {
        public static int Index(this VirtualDesktop desktop) => Array.IndexOf(VirtualDesktop.GetDesktops(), desktop);
    }
}
