﻿using System;
using System.Windows.Media;
using System.Windows.Threading;
using Infrastructure.SharedResources;
using Prism.Ioc;
using Prism.Services.Dialogs;

namespace Timer {
    [Serializable]
    public class OverflowAction : NotifyPropertyChanged {
        private const double FLASH_COLOR_TICK_RATE = 0.5;

        public Guid TimerId { get; set; }
        private ITimer _timer;
        private ITimer Timer => _timer ??= TimersService.Singleton.GetTimer(TimerId);
        public double AfterMinutes { get; set; }
        public bool FlashColorEnabled { get; set; }
        public SolidColorBrush FlashColor { get; set; } = Brushes.Crimson;
        public double FlashColorSeconds { get; set; }
        public bool PlaySoundEnabled { get; set; }
        public string PlaySoundFile { get; set; }
        public bool ShowMessageEnabled { get; set; }
        public string Message { get; set; }

        public event EventHandler<EventArgs> DeleteRequested;
        public void DeleteMe() => DeleteRequested?.Invoke(this, EventArgs.Empty);


        private readonly DispatcherTimer _flashColorTimer = new() {Interval = TimeSpan.FromSeconds(FLASH_COLOR_TICK_RATE)};
        private double _flashColorSecondsLeft;

        private readonly MediaPlayer _mediaPlayer = new();
        private IDialogService _dialogService;

        public OverflowAction(Guid timerID) {
            TimerId = timerID;
            IContainerProvider container = UnityInstance.GetContainer();
            _dialogService = container.Resolve<IDialogService>();

            _flashColorTimer.Tick += (_,  _) => {
                //TODO have to figure out appearance stuff first

                _flashColorSecondsLeft -= FLASH_COLOR_TICK_RATE;
                if(_flashColorSecondsLeft < 0) _flashColorTimer.Stop();
            };
        }

        public void DoAction() {
            if(FlashColorEnabled) {
                _flashColorSecondsLeft = FlashColorSeconds;
                _flashColorTimer.Start();
            }

            if(PlaySoundEnabled) {
                _mediaPlayer.Open(new Uri(PlaySoundFile));
                _mediaPlayer.Play();
            }

            if(ShowMessageEnabled) _dialogService.ShowNotification(Message, Timer.Config.Name, true, false, false);
        }
    }
}