using System;
using System.Windows;

namespace Prio.GlobalServices {
    public interface IVirtualDesktopManager {
        event EventHandler<DesktopChangedEventArgs> DesktopChanged;

        /// <summary> Moves a window to a new virtual desktop </summary>
        /// <param name="window"> window to move </param>
        /// <param name="desktopNum"> 0-indexed desktop number </param>
        void MoveToDesktop(Window window, int desktopNum);

        int CurrentDesktop();
    }

    public class DesktopChangedEventArgs : EventArgs {
        public int OldDesktop { get; }
        public int NewDesktop { get; }

        public DesktopChangedEventArgs(int oldDesktop, int newDesktop) {
            OldDesktop = oldDesktop;
            NewDesktop = newDesktop;
        }
    }
}