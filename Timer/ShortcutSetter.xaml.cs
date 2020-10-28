using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Timer {
    /// <summary>
    /// Interaction logic for ShortcutSetter.xaml
    /// </summary>
    public partial class ShortcutSetter {
        private const string Alt = "Alt";
        private const string Ctrl = "Ctrl";
        private const string Shift = "Shift";
        private const string Win = "Win";

        #region Label DP

        /// <summary>
        /// Gets or sets the Label which is displayed next to the field
        /// </summary>
        public string Label {
            get => (string) GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary>
        /// Identified the Label dependency property
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(ShortcutSetter), new PropertyMetadata(""));

        #endregion

        #region Hint DP

        /// <summary>
        /// Gets or sets the Hint which is displayed over the label
        /// </summary>
        public string Hint {
            get => (string) GetValue(HintProperty);
            set => SetValue(HintProperty, value);
        }

        /// <summary>
        /// Identified the Hint dependency property
        /// </summary>
        public static readonly DependencyProperty HintProperty =
            DependencyProperty.Register(nameof(Hint), typeof(string), typeof(ShortcutSetter), new PropertyMetadata(""));

        #endregion

        #region Shortcut DP

        /// <summary>
        /// Gets or sets the Shortcut which is displayed over the label
        /// </summary>
        public string Shortcut {
            get => (string) GetValue(ShortcutProperty);
            set => SetValue(ShortcutProperty, value);
        }

        /// <summary>
        /// Identified the Shortcut dependency property
        /// </summary>
        public static readonly DependencyProperty ShortcutProperty =
            DependencyProperty.Register(nameof(Shortcut), typeof(string), typeof(ShortcutSetter), new PropertyMetadata(""));

        #endregion

        private ISet<string> _modifiers = new SortedSet<string>();
        private string _key;
        private bool _newFocus;

        public ShortcutSetter() {
            //TODO how to load shortcut string
            InitializeComponent();
            DataContext = this;

            void ResetShortcut() {
                _modifiers = new SortedSet<string>();
                _key = "";
                AltToggle.IsChecked = false;
                CtrlToggle.IsChecked = false;
                ShiftToggle.IsChecked = false;
                WinToggle.IsChecked = false;
                _newFocus = false;
            }

            void UpdateShortcut() => Shortcut = $"{string.Join("+", _modifiers)}{(_modifiers.Count == 0 ? "" : "+")}{_key}";

            GotKeyboardFocus += (o,  e) => _newFocus = true;
            LostKeyboardFocus += (o,  e) => {
                if(string.IsNullOrEmpty(_key)) ResetShortcut();
            };

            var modMap = new Dictionary<string, ToggleButton> {
                {Alt, AltToggle},
                {Ctrl, CtrlToggle},
                {Shift, ShiftToggle},
                {Win, WinToggle}
            };
            foreach(KeyValuePair<string, ToggleButton> pair in modMap) {
                pair.Value.Checked += (o,  e) => {
                    _newFocus = false;
                    _modifiers.Add(pair.Key);
                    UpdateShortcut();
                };
                pair.Value.Unchecked += (o,  e) => {
                    _modifiers.Remove(pair.Key);
                    UpdateShortcut();
                };
            }

            KeyConverter  keyConverter = new KeyConverter();
            Regex removeDirections = new Regex(@"Left|Right|L\B|R+\B");
            Regex modifierMatcher = new Regex(@"Alt|Win|Ctrl|Shift");
            TextInput += (o,  e) => {
                Debug.WriteLine(e.Text);
            };
            KeyDown += (o, e) => {
                if(_newFocus) ResetShortcut();
                if(e.Key == Key.System) e.Handled = true;
                Key key = e.Key == Key.System ? e.SystemKey : e.Key;
                string keyString = keyConverter.ConvertToString(key) ?? string.Empty;
                keyString = removeDirections.Replace(keyString, "");
                if(modifierMatcher.IsMatch(keyString)) {
                    _modifiers.Add(keyString);
                    modMap[keyString].IsChecked = true;
                } else {
                    _key = keyString.Contains("Oem") ? GetCharFromKey(key).ToString().ToUpper() : keyString;
                    DependencyObject scope = FocusManager.GetFocusScope(Parent);
                    FocusManager.SetFocusedElement(scope, Window.GetWindow(this));
                }
                UpdateShortcut();
            };
            KeyUp += (o,  e) => {
                if(e.Key == Key.Back) ResetShortcut();
            };
        }

        private enum MapType : uint {
            MapvkVkToVsc = 0x0
        }

        [DllImport("user32.dll")]
        private static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)] StringBuilder pwszBuff, int cchBuff, uint wFlags);


        [DllImport("user32.dll")] private static extern uint MapVirtualKey(uint uCode, MapType uMapType);

        private static char GetCharFromKey(Key key) {
            char ch = ' ';

            int virtualKey = KeyInterop.VirtualKeyFromKey(key);
            byte[] keyboardState = new byte[256];

            uint scanCode = MapVirtualKey((uint) virtualKey, MapType.MapvkVkToVsc);
            StringBuilder stringBuilder = new StringBuilder(2);

            int result = ToUnicode((uint) virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch(result) {
                case -1:
                    break;
                case 0:
                    break;
                case 1: {
                    ch = stringBuilder[0];
                    break;
                }
                default: {
                    ch = stringBuilder[0];
                    break;
                }
            }
            return ch;
        }
    }
}