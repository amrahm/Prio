using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using static Timer.Shortcut;

namespace Timer {
    /// <summary> Interaction logic for ShortcutSetter.xaml </summary>
    public partial class ShortcutSetter {

        #region Label DP

        /// <summary> Gets or sets the Label which is displayed next to the field </summary>
        public string Label {
            get => (string) GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary> Identifies the Label dependency property </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(ShortcutSetter), new PropertyMetadata(""));

        #endregion

        #region Hint DP

        /// <summary> Gets or sets the Hint which is displayed over the label </summary>
        public string Hint {
            get => (string) GetValue(HintProperty);
            set => SetValue(HintProperty, value);
        }

        /// <summary> Identifies the Hint dependency property </summary>
        public static readonly DependencyProperty HintProperty =
            DependencyProperty.Register(nameof(Hint), typeof(string), typeof(ShortcutSetter), new PropertyMetadata(""));

        #endregion

        #region BoundShortcut DP

        /// <summary> Gets or sets the Shortcut which is displayed over the label </summary>
        public Shortcut BoundShortcut { get; set; }

        #endregion


        /// <summary> Gets or sets the Shortcut which is displayed over the label </summary>
        public string ShortcutString { get; set; }

        private bool _newFocus;

        public ShortcutSetter() {
            //TODO how to load shortcut
            InitializeComponent();
            DataContext = this;

            void ResetShortcut() {
                BoundShortcut.Clear();
                AltToggle.IsChecked = false;
                CtrlToggle.IsChecked = false;
                ShiftToggle.IsChecked = false;
                WinToggle.IsChecked = false;
                _newFocus = false;
            }

            void UpdateShortcut() => ShortcutString = BoundShortcut.ToString();

            GotKeyboardFocus += (o,  e) => _newFocus = true;
            LostKeyboardFocus += (o,  e) => {
                if(BoundShortcut.key == Key.None) ResetShortcut();
            };

            var modToToggle = new Dictionary<KeyType, ToggleButton> {
                {KeyType.Alt, AltToggle},
                {KeyType.Ctrl, CtrlToggle},
                {KeyType.Shift, ShiftToggle},
                {KeyType.Win, WinToggle}
            };

            foreach((KeyType key, ToggleButton value) in modToToggle) {
                value.Checked += (o,  e) => {
                    _newFocus = false;
                    BoundShortcut.Modifiers.Add(key);
                    UpdateShortcut();
                };
                value.Unchecked += (o,  e) => {
                    BoundShortcut.Modifiers.Remove(key);
                    UpdateShortcut();
                };
            }

            KeyDown += (o, e) => {
                if(_newFocus) ResetShortcut();
                if(e.Key == Key.System) e.Handled = true;
                Key key = e.Key == Key.System ? e.SystemKey : e.Key;
                KeyType keyAddedType = BoundShortcut.AddKey(key);
                if(keyAddedType == KeyType.General) {
                    DependencyObject scope = FocusManager.GetFocusScope(Parent);
                    FocusManager.SetFocusedElement(scope, Window.GetWindow(this));
                } else {
                    modToToggle[keyAddedType].IsChecked = true;
                }
                UpdateShortcut();
            };

            KeyUp += (o,  e) => {
                if(e.Key == Key.Back) ResetShortcut();
            };
        }
    }
}