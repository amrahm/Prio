﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shell;
using Infrastructure.SharedResources;
using static Infrastructure.SharedResources.BindingHelpers;
using Color = System.Windows.Media.Color;

namespace Timer {
    /// <summary> Interaction logic for TimerSettingsView.xaml </summary>
    public partial class TimerSettingsView : IDraggable {
        public List<UIElement> ChildDraggables { get; protected internal set; } = new();
        private const int MIN_CTRL_WIDTH = 470;
        private const int SCREEN_MARGIN = 50;
        private const int SNAPPING_INCREMENT = MIN_CTRL_WIDTH + 90;

        private Window _window;
        private double _startHeight;

        public TimerSettingsView() {
            InitializeComponent();

            var vm = (TimerSettingsViewModel) DataContext;

            Loaded += (_, _) => {
                this.InitializeDraggable();

                // Manual Bindings:
                void Callback() => TimersService.Singleton.GetTimer(vm.Config.InstanceID)?.RegisterShortcuts(vm.Config);
                ManualBinding(vm.Config, nameof(vm.Config.ResetShortcut), ResetShortcut,
                              nameof(ResetShortcut.Shortcut), callback: Callback);
                ManualBinding(vm.Config, nameof(vm.Config.StartShortcut), StartShortcut,
                              nameof(StartShortcut.Shortcut), callback: Callback);
                ManualBinding(vm.Config, nameof(vm.Config.StopShortcut), StopShortcut,
                              nameof(StopShortcut.Shortcut), callback: Callback);
                ManualBinding(vm.Config, nameof(vm.Config.ToggleVisibilityShortcut), ToggleVisibilityShortcut,
                              nameof(ToggleVisibilityShortcut.Shortcut), callback: Callback);

                var conditionsVm = new ResetConditionTreeViewModel(vm.Config.ResetConditions);
                ManualBinding(vm.Config, nameof(vm.Config.ResetConditions), conditionsVm, nameof(conditionsVm.Tree));
                RootResetCondition.Content = new ResetConditionTreeView(conditionsVm);

                // Window Setup
                _window = Window.GetWindow(this);
                Debug.Assert(_window != null, nameof(_window) + " != null");

                WindowChrome windowChrome = new() {
                    ResizeBorderThickness = new Thickness(9, 0, 9, 0),
                    CaptionHeight = 0
                };
                WindowChrome.SetWindowChrome(_window, windowChrome);
                _window.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                _startHeight = _window.Height;
                _window.SizeToContent = SizeToContent.Height;

                double dpiScaling = WindowHelpers.GetDpiFactor(_window);

                Rectangle screen = _window.CurrentScreen().WorkingArea;
                while(screen.Height - SCREEN_MARGIN < Root.ActualHeight / dpiScaling &&
                      screen.Width - SCREEN_MARGIN > Root.ActualWidth / dpiScaling + SNAPPING_INCREMENT) {
                    Root.Width += SNAPPING_INCREMENT;
                    _window.Width += SNAPPING_INCREMENT;
                    _window.SizeToContent = SizeToContent.WidthAndHeight;
                }

                _window.MaxWidth = (screen.Width - SCREEN_MARGIN) * dpiScaling;
                _window.MaxHeight = (screen.Height - SCREEN_MARGIN) * dpiScaling;

                _window.CenterOnScreen();
            };

            SizeChanged += (_, e) => {
                const double spacing = 3;

                double newSizeWidth = e.NewSize.Width;
                int maxPerRow = Math.Clamp((int) (newSizeWidth / MIN_CTRL_WIDTH), 1, 3);
                int numPerRow = Math.Min(maxPerRow, MainWrapPanel.Children.Count);
                MainWrapPanel.Width = newSizeWidth + numPerRow * spacing + 20;
                double ctrlWidth = newSizeWidth / numPerRow - (spacing * (numPerRow - 1) + 15) / numPerRow;

                for(int i = 0; i < MainWrapPanel.Children.Count; i++) {
                    FrameworkElement child = (FrameworkElement) MainWrapPanel.Children[i];
                    child.Margin = new Thickness(0, 0, (i + 1) % numPerRow == 0 ? 0 : spacing, spacing);
                    child.Width = ctrlWidth;
                }

                if(_window != null) {
                    _window.SizeToContent = SizeToContent.Height;
                    if(numPerRow == 1) _window.Height = _startHeight;
                }
            };

            ScrollViewer.ScrollChanged += (_,  _) => {
                if(ScrollViewer.ComputedVerticalScrollBarVisibility != Visibility.Visible  ||
                   Math.Abs(ScrollViewer.VerticalOffset + ScrollViewer.ViewportHeight -
                            ScrollViewer.ExtentHeight) < 1)
                    OkBar.ClearValue(EffectProperty);
                else
                    OkBar.Effect = new DropShadowEffect {
                        Color = Colors.Black,
                        Direction = 90,
                        ShadowDepth = 0,
                        Opacity = .5,
                        BlurRadius = 15
                    };
            };


            MouseLeftButtonDown += (_, e) => {
                e.Handled = true;
                DependencyObject scope = FocusManager.GetFocusScope(MainWrapPanel);
                FocusManager.SetFocusedElement(scope, _window);

                if(!ChildDraggables.Any(x => x.IsMouseOver)) _window.DragMove();
            };
        }
    }
}