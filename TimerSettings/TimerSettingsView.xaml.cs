using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using Prism.Services.Dialogs;
using TimerSettings.Annotations;
using Infrastructure.SharedResources;

namespace TimerSettings {
    /// <summary>
    /// Interaction logic for TimerSettingsView.xaml
    /// </summary>
    public partial class TimerSettingsView : INotifyPropertyChanged {
        private const int MinCtrlWidth = 450;
        private const int SnappingIncrement = MinCtrlWidth + 20;
        private double _ctrlWidth = MinCtrlWidth;

        public double CtrlWidth {
            get => _ctrlWidth;
            set {
                _ctrlWidth = value;
                OnPropertyChanged();
            }
        }

        private DialogWindow _dialogWindow;


        private double _startHeight;
        private bool _sizeToContent = true;

        public TimerSettingsView() {
            InitializeComponent();
            Loaded += (o, args) => {
                _dialogWindow = (DialogWindow) Root.Parent;
                WindowChrome windowChrome = new WindowChrome {
                    ResizeBorderThickness = new Thickness(9, 0, 9, 0),
                    CaptionHeight = 0
                };
                WindowChrome.SetWindowChrome(_dialogWindow, windowChrome);
                _dialogWindow.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                _dialogWindow.SizeToContent = SizeToContent.Height;
                _startHeight = _dialogWindow.Height;

                while(_dialogWindow.CurrentScreen().WorkingArea.Height - 50 < _dialogWindow.Height) {
                    if(_dialogWindow.CurrentScreen().WorkingArea.Width - 50 < _dialogWindow.Width + SnappingIncrement) {
                        _sizeToContent = false;
                        break;
                    }
                    _dialogWindow.Width += SnappingIncrement;
                }

                _dialogWindow.MaxHeight = _dialogWindow.CurrentScreen().WorkingArea.Height - 50;
            };

            SizeChanged += (o, args) => {
                double newSizeWidth = args.NewSize.Width;
                int maxPerRow = (int) Math.Floor(newSizeWidth / MinCtrlWidth);
                int numPerRow = Math.Min(maxPerRow, Masonry.Items.Count);
                Masonry.Width = newSizeWidth + numPerRow * Masonry.Spacing + 20;
                CtrlWidth = newSizeWidth / numPerRow - (Masonry.Spacing * (numPerRow - 1) + 15) / (double) numPerRow;
                Masonry.Spacing = numPerRow == 1 ? 3 : 5;
                ConfirmationBar.Margin = new Thickness(0, Masonry.Spacing, 0, 0);

                if(_dialogWindow != null && _sizeToContent) {
                    _dialogWindow.SizeToContent = SizeToContent.Height;
                    if(numPerRow == 1) _dialogWindow.Height = _startHeight;
                }
            };

            MouseDown += (o, args) => {
                if(args.ChangedButton == MouseButton.Left)
                    _dialogWindow.DragMove();
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}