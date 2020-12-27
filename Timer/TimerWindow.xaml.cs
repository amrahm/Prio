using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Timer {
    /// <summary> Interaction logic for TimerWindow.xaml </summary>
    public partial class TimerWindow   {
        public TimerWindow() => InitializeComponent();

        protected override void OnActivated(EventArgs e) {
            base.OnActivated(e);

            //Set the window style to noactivate.
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GWL_EXSTYLE, GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")] private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    }
}