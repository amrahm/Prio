using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using HandyControl.Media.Effects;

namespace MainConfig {
    /// <summary> Interaction logic for NavigationMenuView.xaml </summary>
    public partial class NavigationMenuView   {
        private readonly Storyboard _opacityStoryboard = new();
        private static readonly Duration Duration = new(TimeSpan.FromMilliseconds(200));
        private double _genConfigWidth;
        private double _timersWidth;

        public NavigationMenuView() {
            InitializeComponent();

            Loaded += (_,  _) => {
                _genConfigWidth = GenConfig.ActualWidth;
                _timersWidth = Timers.ActualWidth;
                Timers.Width = MainGrid.ActualWidth;

                GenConfig.IsEnabled = false;
                GenEffect.M22 = 2;
            };

            NavigationMenuViewModel vm = (NavigationMenuViewModel) DataContext;
            vm.PropertyChanged += (_,  args) => {
                if(args.PropertyName == nameof(NavigationMenuViewModel.Selected)) {
                    switch(vm.Selected) {
                        case NavigationMenuViewModel.SelectedButton.GenConfig:
                            SelectButton(GenConfig, GenEffect, _genConfigWidth);
                            break;
                        case NavigationMenuViewModel.SelectedButton.Timers:
                            SelectButton(Timers, TimersEffect, _timersWidth);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            };
        }

        private void SelectButton(Control button, ColorMatrixEffect effect, double width) {
            AnimateButton(button, effect, width, 2);
            button.IsEnabled = false;
            if(button != GenConfig) {
                GenConfig.IsEnabled = true;
                AnimateButton(GenConfig, GenEffect, MainGrid.ActualWidth, 1);
            }
            if(button != Timers) {
                Timers.IsEnabled = true;
                AnimateButton(Timers, TimersEffect, MainGrid.ActualWidth, 1);
            }
        }

        private void AnimateButton(Control button, ColorMatrixEffect effect, double toWidth, double toColor) {
            var animation = new DoubleAnimation {
                From = button.ActualWidth, To = toWidth, Duration = Duration
            };
            Storyboard.SetTarget(animation, button);
            Storyboard.SetTargetProperty(animation, new PropertyPath(WidthProperty));
            _opacityStoryboard.Children.Clear();
            _opacityStoryboard.Children.Add(animation);
            _opacityStoryboard.Begin(this);
            effect.M22 = toColor;
        }
    }
}