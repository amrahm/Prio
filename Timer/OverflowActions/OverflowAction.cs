﻿using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using Infrastructure.SharedResources;
using WindowsDesktop;
using static Infrastructure.SharedResources.UnityInstance;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Drawing.Point;

namespace Timer {
    [Serializable]
    public class OverflowAction : NotifyPropertyChanged {
        private const double FLASH_COLOR_TICK_RATE = 0.3;

        private Guid _timerId;

        public Guid TimerId {
            get => _timerId;
            set {
                _timerId = value;
                _timer = null;
            }
        }

        private ITimer _timer;
        private ITimer Timer => _timer ??= TimersService.Singleton.GetTimer(TimerId);
        public double AfterMinutes { get; set; }
        public bool RepeatEnabled { get; set; }
        public double RepeatMinutes { get; set; }
        public bool FlashColorEnabled { get; set; }
        public SolidColorBrush FlashColor { get; set; } = Brushes.Crimson;
        private SolidColorBrush TextFlashColor => new(FlashColor.Color.Rotate(180));
        public double FlashColorSeconds { get; set; } = 3;
        public bool PlaySoundEnabled { get; set; }
        public string PlaySoundFile { get; set; }
        public bool ShowMessageEnabled { get; set; }
        public string Message { get; set; }
        public bool ForceOffDesktop { get; set; }

        public event EventHandler<EventArgs> DeleteRequested;
        public void DeleteMe() => DeleteRequested?.Invoke(this, EventArgs.Empty);


        private readonly DispatcherTimer _flashColorTimer = new() {Interval = TimeSpan.FromSeconds(FLASH_COLOR_TICK_RATE)};
        private double _flashColorSecondsLeft;

        private readonly MediaPlayer _mediaPlayer = new();

        public OverflowAction(Guid timerID) {
            TimerId = timerID;

            _flashColorTimer.Tick += (_,  _) => {
                Timer.TempBackgroundBrush = Timer.TempBackgroundBrush == null ? FlashColor : null;
                Timer.TempTextBrush = Timer.TempTextBrush == null ? TextFlashColor : null;

                _flashColorSecondsLeft -= FLASH_COLOR_TICK_RATE;
                if(_flashColorSecondsLeft < 0) {
                    Timer.TempBackgroundBrush = null;
                    Timer.TempTextBrush = null;
                    _flashColorTimer.Stop();
                }
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

            if(ShowMessageEnabled) {
                var pos = Timer.Config.WindowPositions[Screen.AllScreens.Count()];
                Dialogs.ShowNotification(Message, Timer.Config.Name, true, false, false,
                                         Screen.FromPoint(new Point((int) pos.X, (int) pos.Y)));
            }

            if(RepeatEnabled)
                Timer.AddTimerAction(new TimerAction(Timer.Config.TimeLeft - TimeSpan.FromMinutes(RepeatMinutes), DoAction),
                                     true);

            if(ForceOffDesktop) {
                VDM.DesktopChanged += VDMOnDesktopChanged;
                if(Timer.Config.DesktopsActive.Contains(VDM.CurrentDesktop())) {
                    for(int i = 0; i < VDM.NumDesktops(); i++) {
                        if(Timer.Config.DesktopsActive.Contains(i)) continue;
                        VDM.SwitchToDesktop(i);
                        break;
                    }
                }
            } else {
                VDM.DesktopChanged -= VDMOnDesktopChanged;
            }
        }

        private void VDMOnDesktopChanged(object sender, VirtualDesktopChangedEventArgs e) {
            if(Timer.Config.TimeLeft.Ticks <= 0 && Timer.Config.DesktopsActive.Contains(e.NewDesktop.Index)) {
                VDM.SwitchToDesktop(e.OldDesktop.Index);
            }
        }
    }
}
