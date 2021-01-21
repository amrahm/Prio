using System.Windows;
using System.Windows.Input;

namespace Infrastructure.SharedResources {
    /// <summary> Interaction logic for ColorPickerView.xaml </summary>
    public partial class ColorPickerView {
        public ColorPickerView() {
            InitializeComponent();

            ColorPickerViewModel vm = (ColorPickerViewModel) DataContext;
            vm.Picker = ColorPicker;

            Loaded += (_, _) => {
                Window window = Window.GetWindow(this);
                if(window == null) return;

                Point mousePos = ColorPicker.PointToScreen(Mouse.GetPosition(ColorPicker));
                window.Top = mousePos.Y - window.ActualHeight / 2;
                window.Left = mousePos.X - window.ActualWidth / 2;
                window.MoveWindowInBounds();
            };
        }
    }
}