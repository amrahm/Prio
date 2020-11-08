﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using Infrastructure.SharedResources;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Application = System.Windows.Application;

namespace Timer {
    /// <summary> Interaction logic for TimerView.xaml </summary>
    public partial class TimerView  {
        private Window _window;
        private readonly TimerViewModel _vm;

        public TimerView() {
            InitializeComponent();
            SizeChanged += TimerAspectRatioLimits;

            _vm = (TimerViewModel) DataContext;

            Loaded += (o, e) => {
                _window = Window.GetWindow(this);
                Debug.Assert(_window != null, nameof(_window) + " != null");
                Debug.WriteLine(Application.Current.Windows);

                WindowChrome windowChrome = new WindowChrome {CaptionHeight = 0};
                WindowChrome.SetWindowChrome(_window, windowChrome);
                _window.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                LoadWindowPosition();

                void UnHide() {
                    _window.Visibility = Visibility.Visible;
                    _window.Activate();
                }

                void HideToggle() {
                    Debug.WriteLine("HIDE_TOGGLE");
                    if(_window.Visibility != Visibility.Visible) UnHide();
                    else _window.Visibility = Visibility.Hidden;
                }

                _vm.Timer.RequestHide += HideToggle;

                void KeepOnTop() {
                    Debug.WriteLine("TOP");
                    UnHide();
                    _window.Topmost = true;
                }

                _vm.Timer.RequestKeepOnTop += KeepOnTop;

                void MoveToBottom() {
                    Debug.WriteLine("BOTTOM");
                    UnHide();
                    _window.Topmost = false;
                    IntPtr hWnd = new WindowInteropHelper(_window).Handle;
                    SetWindowPos(hWnd, HwndBottom, 0, 0, 0, 0, 19U);
                }

                _vm.Timer.RequestMoveBelow += MoveToBottom;

                switch(_vm.Timer.Config.VisibilityState) {
                    case VisibilityState.Hidden:
                        HideToggle();
                        break;
                    case VisibilityState.KeepOnTop:
                        KeepOnTop();
                        break;
                    case VisibilityState.MoveBehind:
                        MoveToBottom();
                        break;
                }
            };

            MouseDown += DragMoveWindow;
        }

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X,
            int Y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HwndBottom = new IntPtr(1);

        private void DragMoveWindow(object o, MouseButtonEventArgs e) {
            if(_window == null) return;
            if(e.ChangedButton == MouseButton.Left) {
                DependencyObject scope = FocusManager.GetFocusScope(Root);
                FocusManager.SetFocusedElement(scope, _window);

                _window.DragMove();

                Point newPos = new Point(_window.Left, _window.Top);
                if(!_vm.Timer.Config.WindowPositions.TryGetValue(Screen.AllScreens.Length, out Point configPos) ||
                   !configPos.Equals(newPos)) {
                    _vm.Timer.Config.WindowPositions[Screen.AllScreens.Length] = newPos;
                    _vm.Timer.SaveSettings();
                }
            }
        }

        private void LoadWindowPosition() {
            if(_vm.Timer.Config.WindowPositions.Count > 0) {
                Point? configWindowPosition = null;
                for(int i = Screen.AllScreens.Length; i > 0; i--)
                    if(_vm.Timer.Config.WindowPositions.ContainsKey(i))
                        configWindowPosition = _vm.Timer.Config.WindowPositions[Screen.AllScreens.Length];

                if(configWindowPosition != null) {
                    _window.Left = configWindowPosition.Value.X;
                    _window.Top = configWindowPosition.Value.Y;
                }
            }

            PresentationSource mainWindowPresentationSource = PresentationSource.FromVisual(_window);
            Debug.Assert(mainWindowPresentationSource != null, nameof(mainWindowPresentationSource) + " != null");
            Debug.Assert(mainWindowPresentationSource.CompositionTarget != null, "CompositionTarget != null");
            Matrix m = mainWindowPresentationSource.CompositionTarget.TransformToDevice;
            double dpiWidthFactor = m.M11;
            double dpiHeightFactor = m.M22;

            Rectangle wA = _window.CurrentScreen().WorkingArea;

            _window.Left += Math.Max(wA.Left / dpiWidthFactor - _window.Left, 0);
            _window.Left += Math.Min(wA.Right / dpiWidthFactor - (_window.Left + _window.ActualWidth), 0);
            _window.Top += Math.Max(wA.Top / dpiHeightFactor - _window.Top, 0);
            _window.Top += Math.Min(wA.Bottom / dpiHeightFactor - (_window.Top + _window.ActualHeight), 0);

            foreach(ITimer timer in TimersService.Singleton.Timers) {
                //TODO move this out of the way if there's an overlap
            }
        }

        private void TimerAspectRatioLimits(object sender, SizeChangedEventArgs sizeChangedEventArgs) {
            double hToW = TimerText.ActualWidth / TimerText.ActualHeight;
            TimerViewbox.MaxHeight = TimerViewbox.ActualWidth / hToW * 1.5;
            TimerViewbox.MaxWidth = TimerViewbox.ActualHeight * hToW * 1.5;
        }
    }
}