using System;
using System.Linq;
using Infrastructure.SharedResources;
using System.Windows.Media;
using HandyControl.Controls;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;

namespace Timer {
    public class OverflowActionViewModel : NotifyPropertyWithDependencies {
        private readonly OverflowAction _model;
        public OverflowAction Model {
            get => _model;
            private init => NotificationBubbler.BubbleSetter(ref _model, value, (_, _) => this.OnPropertyChanged());
        }

        private readonly MediaPlayer _mediaPlayer = new();
        public DelegateCommand SelectColorCommand { get; }
        public DelegateCommand SelectSoundCommand { get; }
        public DelegateCommand DeleteCommand { get; }

        public OverflowActionViewModel(OverflowAction model) {
            Model = model;
            IDialogService dialogService = UnityInstance.GetContainer().Resolve<IDialogService>();

            SelectColorCommand = new DelegateCommand(() => {
                var r = dialogService.ShowColorPicker(Model.FlashColor).Result;
                if(r.Result == ButtonResult.OK)
                    Model.FlashColor = r.Parameters.GetValue<SolidColorBrush>(nameof(ColorPicker.SelectedBrush));
            });

            SelectSoundCommand = new DelegateCommand(() => {
                OpenFileDialog openFileDialog = new() {
                    Filter = "Audio files (*.mp3, *.wav, etc.)|*.mp3;*.wav;*.aac;*.pcm;*.ogg;*.aiff;*.wma;*.flac;*.alac"
                };
                _mediaPlayer.Stop();
                if(openFileDialog.ShowDialog() == true) {
                    _mediaPlayer.Open(new Uri(openFileDialog.FileName));
                    _mediaPlayer.Play();
                    Model.PlaySoundFile = openFileDialog.FileName;
                }
            });

            DeleteCommand = new DelegateCommand(() => Model.DeleteMe());
        }

        [DependsOnProperty(nameof(Model))]
        public string PlaySoundFile => Model == null || string.IsNullOrEmpty(Model.PlaySoundFile) ?
                null :
                $"...\\{Model.PlaySoundFile.Split('\\').Last()}";
    }
}