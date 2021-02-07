using System;
using System.Windows;

namespace Prio.GlobalServices {
    public interface IVirtualDesktopManager {
        event EventHandler<EventArgs> DesktopChanged;

        /// <summary> Moves a window to a new virtual desktop </summary>
        /// <param name="window"> window to move </param>
        /// <param name="desktopNum"> 0-indexed desktop number </param>
        void MoveToDesktop(Window window, int desktopNum);

        int CurrentDesktop();
    }
}