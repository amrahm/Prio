using System;
using HandyControl.Controls;
using Prism.Services.Dialogs;

namespace Infrastructure.SharedResources {
    public class ColorPickerViewModel : IDialogAware {
        public string Title { get; } = "Color Picker";
        public event Action<IDialogResult> RequestClose;
        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }
        public void OnDialogOpened(IDialogParameters parameters) { }

        private ColorPicker _picker;
        public ColorPicker Picker {
            set {
                _picker = value;
                _picker.SelectedColorChanged += delegate {
                    RequestClose?.Invoke(new DialogResult(ButtonResult.OK, new DialogParameters {
                        {nameof(ColorPicker.SelectedBrush), _picker.SelectedBrush}
                    }));
                };
                _picker.Canceled += delegate { RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)); };
            }
        }
    }
}