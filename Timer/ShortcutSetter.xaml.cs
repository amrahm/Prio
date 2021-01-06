using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Infrastructure.SharedResources;
using PropertyChanged;
using static Infrastructure.SharedResources.ShortcutDefinition;

namespace Timer {
    /// <summary> Interaction logic for ShortcutSetter.xaml </summary>
    public partial class ShortcutSetter : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

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

        /// <summary> Gets or sets the ShortcutDefinition which is displayed over the label </summary>
        public ShortcutDefinition Shortcut {
            get => _shortcut;
            set {
                _shortcut = value;
                UpdateShortcutString();
                if(_shortcut != null)
                    foreach((ModifierKeys key, ToggleButton val) in _modToToggle)
                        val.IsChecked = (_shortcut.Modifiers | key) == _shortcut.Modifiers;
            }
        }

        /// <summary> Gets or sets the ShortcutDefinition which is displayed over the label </summary>
        [DependsOn(nameof(Shortcut))]
        public string ShortcutString { get; private set; }

        private bool _newFocus;
        private ShortcutDefinition _shortcut = new();
        private readonly Dictionary<ModifierKeys, ToggleButton> _modToToggle;

        public ShortcutSetter() {
            InitializeComponent();
            DataContext = this;

            _modToToggle = new Dictionary<ModifierKeys, ToggleButton> {
                {ModifierKeys.Alt, AltToggle},
                {ModifierKeys.Control, CtrlToggle},
                {ModifierKeys.Shift, ShiftToggle},
                {ModifierKeys.Windows, WinToggle}
            };

            GotKeyboardFocus += (_, _) => {
                _newFocus = true;
                Shortcut ??= new ShortcutDefinition();
            };
            LostKeyboardFocus += (_, _) => {
                if(Shortcut.Key == Key.None) Shortcut = null;
            };

            foreach((ModifierKeys key, ToggleButton value) in _modToToggle) {
                value.Checked += (_, _) => {
                    _newFocus = false;
                    Shortcut = Shortcut.WithKey(key);
                    UpdateShortcutString();
                };
                value.Unchecked += (_, _) => {
                    Shortcut = Shortcut.WithoutKey(key);
                    UpdateShortcutString();
                };
            }

            KeyDown += (_, e) => {
                if(_newFocus) {
                    Shortcut = new ShortcutDefinition();
                    _newFocus = false;
                }
                if(e.Key == Key.System) e.Handled = true;
                Key key = e.Key == Key.System ? e.SystemKey : e.Key;
                Shortcut = Shortcut.WithKey(key);

                if(ModifierKeysMap.TryGetValue(key, out ModifierKeys keyAddedType)) {
                    _modToToggle[keyAddedType].IsChecked = true;
                } else {
                    DependencyObject scope = FocusManager.GetFocusScope(Parent);
                    FocusManager.SetFocusedElement(scope, Window.GetWindow(this));
                }
                UpdateShortcutString();
            };

            KeyUp += (_,  e) => {
                if(e.Key == Key.Back) Shortcut = new ShortcutDefinition();
            };
        }

        private void UpdateShortcutString() => ShortcutString = Shortcut?.ToString();
    }
}