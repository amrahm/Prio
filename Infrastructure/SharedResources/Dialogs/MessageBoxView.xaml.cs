﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using Window = System.Windows.Window;

namespace Infrastructure.SharedResources {
    /// <summary> Interaction logic for ShowMessageView.xaml </summary>
    public partial class MessageBoxView {
        private Window _window;

        public MessageBoxView() {
            InitializeComponent();

            MessageBoxViewModel vm = (MessageBoxViewModel) DataContext;
            vm.PropertyChanged += (_,  args) => {
                if(args.PropertyName == nameof(vm.Message)) InlineExpression.SetInlineExpression(MessageBlock, vm.Message);
            };

            Loaded += (_, _) => {
                // Window Setup
                _window = Window.GetWindow(this);
                Debug.Assert(_window != null, nameof(_window) + " != null");

                _window.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                Rectangle screen = vm.openOnScreen?.WorkingArea ?? _window.CurrentScreen().WorkingArea;
                _window.CenterOnScreen(screen);


                //Set the window style to noactivate.
                if(!vm.getsFocus) {
                    var helper = new WindowInteropHelper(_window);
                    SetWindowLong(helper.Handle, GWL_EXSTYLE, GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
                }
            };

            MouseDown += (_, e) => {
                if(e.ChangedButton == MouseButton.Left) _window.DragMove();
            };
        }


        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        [DllImport("user32.dll")] private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    }
}