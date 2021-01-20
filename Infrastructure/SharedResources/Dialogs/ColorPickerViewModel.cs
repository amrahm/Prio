using System;
using System.Threading.Tasks;
using System.Windows.Media;
using HandyControl.Controls;
using Prism.Services.Dialogs;

namespace Infrastructure.SharedResources {
    public class ColorPickerViewModel : IDialogAware {
        public string Title { get; } = "Color Picker";
        public event Action<IDialogResult> RequestClose;
        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters) {
            SolidColorBrush brush = parameters.GetValue<SolidColorBrush>(nameof(ColorPicker.SelectedBrush));
            if(brush != null) _picker.SelectedBrush = brush;
        }

        private ColorPicker _picker;
        public ColorPicker Picker {
            set {
                _picker = value;
                _picker.SelectedColorChanged += (_, _) => RequestClose?.Invoke(
                    new DialogResult(ButtonResult.OK, new DialogParameters {
                        {nameof(ColorPicker.SelectedBrush), _picker.SelectedBrush}
                    }));
                _picker.Canceled += (_, _) => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
            }
        }
    }

    public static class DialogServiceColorPickerExtension {
        public static Task<IDialogResult> ShowColorPicker(this IDialogService dialogService,
                                                          SolidColorBrush selectedBrush = null) {
            return dialogService.ShowDialogAsync(nameof(ColorPickerView),
                                                 new DialogParameters {{nameof(ColorPicker.SelectedBrush), selectedBrush}});
        }
    }
}