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
        public enum ModifierType { Alt, Ctrl, Shift, Win }

        public static readonly ImmutableDictionary<Key, ModifierType> ModifierTypeMap = new Dictionary<Key, ModifierType> {
            {Key.LeftAlt, ModifierType.Alt}, {Key.RightAlt, ModifierType.Alt},
            {Key.LeftCtrl, ModifierType.Ctrl}, {Key.RightCtrl, ModifierType.Ctrl},
            {Key.LeftShift, ModifierType.Shift}, {Key.RightShift, ModifierType.Shift},
            {Key.LWin, ModifierType.Win},  {Key.RWin, ModifierType.Win}
        }.ToImmutableDictionary();


        public Key Key { get; }
        public ImmutableHashSet<ModifierType> Modifiers { get; } = ImmutableHashSet<ModifierType>.Empty;

        public ShortcutDefinition(Key key = Key.None, IEnumerable<ModifierType> modifiers = null) {
            Key = key;
            Modifiers = Modifiers.Union(modifiers ?? Array.Empty<ModifierType>());
        }

        public ShortcutDefinition WithKey(Key newKey) {
            return ModifierTypeMap.TryGetValue(newKey, out ModifierType keyType) ?
                       new ShortcutDefinition(Key, Modifiers.Add(keyType)) :
                       new ShortcutDefinition(newKey, Modifiers);
        }

        public ShortcutDefinition WithKey(ModifierType mod) => new ShortcutDefinition(Key, Modifiers.Add(mod));


        public ShortcutDefinition WithoutKey(Key newKey) {
            return ModifierTypeMap.TryGetValue(newKey, out ModifierType keyType) ?
                       new ShortcutDefinition(Key, Modifiers.Remove(keyType)) :
                       new ShortcutDefinition(Key.None, Modifiers);
        }

        public ShortcutDefinition WithoutKey(ModifierType mod) => new ShortcutDefinition(Key, Modifiers.Remove(mod));


        public override bool Equals(object obj) =>
            obj is ShortcutDefinition other && Key == other.Key && Modifiers == other.Modifiers;

        public override int GetHashCode() => Key.GetHashCode() + Modifiers.GetHashCode();


        private static readonly KeyConverter  KeyConverter = new KeyConverter();

        public override string ToString() {
            string keyString = KeyConverter.ConvertToString(Key) ?? string.Empty;
            keyString = keyString.Contains("Oem") ? GetCharFromKey(Key).ToString().ToUpper() : keyString;
            return $"{string.Join("+", Modifiers)}{(Modifiers.Count == 0 ? "" : "+")}{keyString}";
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