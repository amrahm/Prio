using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
using Prism.Mvvm;

namespace Infrastructure.SharedResources {
    /// <summary> An immutable shortcut definition </summary>
    [Serializable]
    public class ShortcutDefinition : BindableBase {
        public static readonly ImmutableDictionary<Key, ModifierKeys> ModifierKeysMap = new Dictionary<Key, ModifierKeys> {
            {Key.LeftAlt, ModifierKeys.Alt}, {Key.RightAlt, ModifierKeys.Alt},
            {Key.LeftCtrl, ModifierKeys.Control}, {Key.RightCtrl, ModifierKeys.Control},
            {Key.LeftShift, ModifierKeys.Shift}, {Key.RightShift, ModifierKeys.Shift},
            {Key.LWin, ModifierKeys.Windows},  {Key.RWin, ModifierKeys.Windows}
        }.ToImmutableDictionary();


        public Key Key { get; }
        public ModifierKeys Modifiers { get; }

        public ShortcutDefinition(Key key = Key.None, ModifierKeys modifiers = ModifierKeys.None) {
            Key = key;
            Modifiers = modifiers;
        }

        public ShortcutDefinition WithKey(Key newKey) {
            return ModifierKeysMap.TryGetValue(newKey, out ModifierKeys mod) ?
                       new ShortcutDefinition(Key, Modifiers | mod) :
                       new ShortcutDefinition(newKey, Modifiers);
        }

        public ShortcutDefinition WithKey(ModifierKeys mod) => new ShortcutDefinition(Key, Modifiers | mod);


        public ShortcutDefinition WithoutKey(Key newKey) {
            return ModifierKeysMap.TryGetValue(newKey, out ModifierKeys mod) ?
                       new ShortcutDefinition(Key, Modifiers & ~mod) :
                       new ShortcutDefinition(Key.None, Modifiers);
        }

        public ShortcutDefinition WithoutKey(ModifierKeys mod) => new ShortcutDefinition(Key, Modifiers & ~mod);


        public override bool Equals(object obj) =>
            obj is ShortcutDefinition other && Key == other.Key && Modifiers == other.Modifiers;

        public override int GetHashCode() => HashCode.Combine(Key, Modifiers);


        private static readonly KeyConverter  KeyConverter = new KeyConverter();

        public override string ToString() {
            string keyString = KeyConverter.ConvertToString(Key) ?? string.Empty;
            keyString = keyString.Contains("Oem") ? GetCharFromKey(Key).ToString().ToUpper() : keyString;
            return
                $"{Modifiers.ToString().Replace(", ", "+").Replace("Control", "Ctrl").Replace("Windows", "Win").Replace("None", "")}" +
                $"{(Modifiers == ModifierKeys.None ? "" : "+")}{keyString}";
        }


        private enum MapType : uint { MapvkVkToVsc = 0x0 }

        [DllImport("user32.dll")]
        private static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)] StringBuilder pwszBuff, int cchBuff, uint wFlags);


        [DllImport("user32.dll")] private static extern uint MapVirtualKey(uint uCode, MapType uMapType);

        private static char GetCharFromKey(Key key) {
            int virtualKey = KeyInterop.VirtualKeyFromKey(key);

            uint scanCode = MapVirtualKey((uint) virtualKey, MapType.MapvkVkToVsc);
            StringBuilder stringBuilder = new StringBuilder(2);

            int result = ToUnicode((uint) virtualKey, scanCode, new byte[256], stringBuilder, stringBuilder.Capacity, 0);
            if(result == -1 || result == 0) return ' ';
            return stringBuilder[0];
        }
    }
}