﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using Infrastructure.SharedResources;
using Prism.Services.Dialogs;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace Timer {
    /// <summary> Interaction logic for TimerView.xaml </summary>
    public partial class TimerView  {
        private Window _window;
        private readonly TimerViewModel _vm;

        public TimerView(TimerViewModel vm = null) {
            InitializeComponent();
            SizeChanged += TimerAspectRatioLimits;

            if(vm != null) DataContext = vm;
            _vm = (TimerViewModel) DataContext;

            Loaded += (o, e) => {
                _window = Window.GetWindow(this);
                if(!(_window is TimerWindow)) return; //This isn't a floating timer

                WindowChrome windowChrome = new WindowChrome {CaptionHeight = 0};
                WindowChrome.SetWindowChrome(_window, windowChrome);
                _window.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                LoadWindowPosition();
            };

            Unloaded += (o, e) => _vm.Timer.TimerWindow = null;

            MouseDown += DragMoveWindow;

            SizeChanged += (o,  e) => SaveWindowPosition();
        }

        private void DragMoveWindow(object o, MouseButtonEventArgs e) {
            if(_window == null) return;
            if(e.ChangedButton == MouseButton.Left) {
                DependencyObject scope = FocusManager.GetFocusScope(Root);
                FocusManager.SetFocusedElement(scope, _window);

                _window.DragMove();

                SaveWindowPosition();
            }
        }

        private void SaveWindowPosition() {
            if(_window == null) return;
            WindowPosition newPos =
                new WindowPosition(_window.Left, _window.Top, _window.ActualWidth, _window.ActualHeight);
            if(!_vm.Timer.Config.WindowPositions.TryGetValue(Screen.AllScreens.Length, out WindowPosition configPos) ||
               !configPos.Equals(newPos)) {
                _vm.Timer.Config.WindowPositions[Screen.AllScreens.Length] = newPos;
                _vm.Timer.SaveSettings();
            }
        }

        private void LoadWindowPosition() {
            if(_vm.Timer.Config.WindowPositions.Count > 0) {
                WindowPosition? configWindowPosition = null;
                for(int i = Screen.AllScreens.Length; i > 0; i--)
                    if(_vm.Timer.Config.WindowPositions.ContainsKey(i))
                        configWindowPosition = _vm.Timer.Config.WindowPositions[Screen.AllScreens.Length];

                if(configWindowPosition != null) {
                    _window.Left = configWindowPosition.Value.X;
                    _window.Top = configWindowPosition.Value.Y;
                    _window.Width = configWindowPosition.Value.Width;
                    _window.Height = configWindowPosition.Value.Height;
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