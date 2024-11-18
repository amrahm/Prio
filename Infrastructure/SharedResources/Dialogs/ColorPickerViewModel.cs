using System.Threading.Tasks;
using System.Windows.Media;
using HandyControl.Controls;
using JetBrains.Annotations;
using Prism.Dialogs;

namespace Infrastructure.SharedResources {
    public class ColorPickerViewModel : IDialogAware {
        public string Title { get; } = "Color Picker";
        [UsedImplicitly] public DialogCloseListener RequestClose { get; }
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
                _picker.Confirmed += (_, _) => RequestClose.Invoke(new DialogParameters {
                                                                       {
                                                                           nameof(ColorPicker.SelectedBrush),
                                                                           _picker.SelectedBrush
                                                                       }
                                                                   },
                                                                   ButtonResult.OK);
                _picker.Canceled += (_, _) => RequestClose.Invoke(ButtonResult.Cancel);
            }
        }
    }

    public static class DialogServiceColorPickerExtension {
        public static Task<IDialogResult> ShowColorPicker(this IDialogService dialogService,
                                                          SolidColorBrush selectedBrush) {
            return dialogService.ShowDialogAsync(nameof(ColorPickerView),
                                                 new DialogParameters {{nameof(ColorPicker.SelectedBrush), selectedBrush}});
        }
    }
}
