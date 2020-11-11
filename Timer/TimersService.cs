using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Interop;
using Infrastructure.Constants;
using Infrastructure.SharedResources;
using Newtonsoft.Json;
using Prio.GlobalServices;
using Prism.Ioc;

namespace Timer {
    [Serializable]
    public class TimersService : NotifyPropertyChanged {
        [NonSerialized]
        public static readonly TimersService Singleton =
            Settings.LoadSettings<TimersService>(ModuleNames.TIMER) ?? new TimersService();

        public TimersGeneralConfig GeneralConfig { get; set; } = new TimersGeneralConfig();

        private class TimerConverter : JsonConverter<ITimer> {
            public override ITimer ReadJson(JsonReader reader, Type objectType, ITimer existingValue, bool hasExistingValue,
                JsonSerializer serializer) =>
                new TimerModel(serializer.Deserialize<TimerConfig>(reader));

            public override void WriteJson(JsonWriter writer, ITimer value, JsonSerializer serializer) =>
                serializer.Serialize(writer, value.Config);
        }

        [JsonProperty(ItemConverterType = typeof(TimerConverter))]
        public ObservableCollection<ITimer> Timers { get; } = new ObservableCollection<ITimer>();

        public void SaveSettings() {
            Settings.SaveSettings(this, ModuleNames.TIMER);
            RegisterShortcuts();
        }

        private enum VisibilityHotkeyState { ShouldHide, ShouldTop, ShouldBehind }


        private VisibilityState _currVisState = VisibilityState.KeepOnTop;
        private VisibilityState _lastNonHiddenVisState = VisibilityState.KeepOnTop;

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context) {
            RegisterShortcuts();
            _currVisState = GeneralConfig.DefaultVisibilityState;
        }

        public void ApplyVisState() {
            switch(_currVisState) {
                case VisibilityState.KeepOnTop:
                    TopAll();
                    break;
                case VisibilityState.MoveBehind:
                    BottomAll();
                    break;
                case VisibilityState.Hidden:
                    ShowHideAll();
                    break;
            }
        }

        private void RegisterShortcuts() {
            IContainerProvider container = UnityInstance.GetContainer();
            var hotkeyManager = container.Resolve<IPrioHotkeyManager>();

            bool hideIsTop = Equals(GeneralConfig.ShowHideTimersShortcut, GeneralConfig.KeepTimersOnTopShortcut);
            bool hideIsBottom = Equals(GeneralConfig.ShowHideTimersShortcut, GeneralConfig.MoveTimersBehindShortcut);
            bool topIsBottom = Equals(GeneralConfig.MoveTimersBehindShortcut, GeneralConfig.KeepTimersOnTopShortcut);

            int NextVisibilityState(int r) {
                bool isHidden = _currVisState == VisibilityState.Hidden;
                bool isTop = _currVisState == VisibilityState.KeepOnTop;
                bool isBottom = _currVisState == VisibilityState.MoveBehind;
                switch((VisibilityHotkeyState) r) { //Find all cases where we shouldn't do the requested action
                    // These set precedence so that we move in a triangle if needed
                    // They also check if we are requesting to do what we already are
                    case VisibilityHotkeyState.ShouldHide when hideIsBottom && isTop:
                    case VisibilityHotkeyState.ShouldTop when isTop || hideIsTop && isBottom:
                    case VisibilityHotkeyState.ShouldBehind when isBottom || topIsBottom && isHidden:

                    // If we are currently in a showhide state, however, we might want to repeat the action
                    // But only if there isn't another visibility action with the same key
                    case VisibilityHotkeyState.ShouldHide when isHidden && (hideIsTop || hideIsBottom):
                        return -1;
                }
                return r; //otherwise, do the requested action
            }

            hotkeyManager.RegisterHotkey(nameof(GeneralConfig.ShowHideTimersShortcut), GeneralConfig.ShowHideTimersShortcut,
                                         ShowHideAll, CompatibilityType.Visibility,
                                         (int) VisibilityHotkeyState.ShouldHide, NextVisibilityState);

            hotkeyManager.RegisterHotkey(nameof(GeneralConfig.KeepTimersOnTopShortcut),
                                         GeneralConfig.KeepTimersOnTopShortcut,
                                         TopAll, CompatibilityType.Visibility,
                                         (int) VisibilityHotkeyState.ShouldTop, NextVisibilityState);

            hotkeyManager.RegisterHotkey(nameof(GeneralConfig.MoveTimersBehindShortcut),
                                         GeneralConfig.MoveTimersBehindShortcut,
                                         BottomAll, CompatibilityType.Visibility,
                                         (int) VisibilityHotkeyState.ShouldBehind, NextVisibilityState);
        }

        private void ShowHideAll() {
            Visibility target;
            if(_currVisState != VisibilityState.Hidden) {
                target = Visibility.Hidden;
                _currVisState = VisibilityState.Hidden;
            } else {
                target = Visibility.Visible;
                _currVisState = _lastNonHiddenVisState;
            }
            foreach(ITimer timer in Timers)
                if(timer.OpenWindow != null) {
                    timer.OpenWindow.Visibility = target;
                    if(target == Visibility.Visible) timer.OpenWindow.Activate();
                }
        }

        private void TopAll() {
            _currVisState = VisibilityState.KeepOnTop;
            _lastNonHiddenVisState = VisibilityState.KeepOnTop;
            foreach(ITimer timer in Timers) {
                if(timer.OpenWindow != null) {
                    timer.OpenWindow.Visibility = Visibility.Visible;
                    timer.OpenWindow.Topmost = true;
                }
            }
        }

        private void BottomAll() {
            _currVisState = VisibilityState.MoveBehind;
            _lastNonHiddenVisState = VisibilityState.MoveBehind;
            foreach(ITimer timer in Timers) {
                if(timer.OpenWindow != null) {
                    timer.OpenWindow.Visibility = Visibility.Visible;
                    timer.OpenWindow.Topmost = false;

                    var hWnd = new WindowInteropHelper(timer.OpenWindow).Handle;
                    SetWindowPos(hWnd, new IntPtr(1), 0, 0, 0, 0, 19U);
                }
            }
        }

        [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y,
            int cx, int cy, uint uFlags);
    }
}