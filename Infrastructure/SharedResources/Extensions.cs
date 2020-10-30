﻿using System.Windows;
using System.Windows.Forms;

namespace Infrastructure.SharedResources {
    public static class WindowHelpers {
        public static Screen CurrentScreen(this Window window) {
            return Screen.FromPoint(new System.Drawing.Point((int) window.Left, (int) window.Top));
        }
    }
}