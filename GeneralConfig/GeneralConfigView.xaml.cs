using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace GeneralConfig {
    /// <summary> Interaction logic for GeneralConfigView.xaml </summary>
    public partial class GeneralConfigView {
        public GeneralConfigView() {
            InitializeComponent();

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
        }
    }
}