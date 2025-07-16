using System;
using System.Windows;
using WindowsDesktop;

namespace Prio.GlobalServices {
    public interface IVirtualDesktopManager {
        event EventHandler<VirtualDesktopChangedEventArgs> DesktopChanged;

        /// <summary> Moves a window to a new virtual desktop </summary>
        /// <param name="window"> window to move </param>
        /// <param name="desktopNum"> 0-indexed desktop number </param>
        void MoveToDesktop(Window window, int desktopNum);

        /// <summary> Moves to a virtual desktop </summary>
        /// <param name="desktopNum"> 0-indexed desktop number </param>
        void SwitchToDesktop(int desktopNum);

        int CurrentDesktop();

        int NumDesktops();
    }
}
