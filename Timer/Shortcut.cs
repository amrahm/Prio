using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
using Prism.Mvvm;

namespace Timer {
    [Serializable] 
    public class Shortcut : BindableBase {
        public enum KeyType { General, Alt, Ctrl, Shift, Win }

        private static readonly Dictionary<Key, KeyType> ModMap = new Dictionary<Key, KeyType> {
            {Key.LeftAlt, KeyType.Alt}, {Key.RightAlt, KeyType.Alt},
            {Key.LeftCtrl, KeyType.Ctrl}, {Key.RightCtrl, KeyType.Ctrl},
            {Key.LeftShift, KeyType.Shift}, {Key.RightShift, KeyType.Shift},
            {Key.LWin, KeyType.Win},  {Key.RWin, KeyType.Win}
        };


        public Key Key { get; set; }
        public ISet<KeyType> Modifiers { get; } = new SortedSet<KeyType>();

        public KeyType AddKey(Key newKey) {
            if(ModMap.TryGetValue(newKey, out KeyType keyType)) {
                Modifiers.Add(keyType);
            } else Key = newKey;
            return keyType;
        }

        public KeyType RemoveKey(Key newKey) {
            if(ModMap.TryGetValue(newKey, out KeyType keyType)) {
                Modifiers.Remove(keyType);
            } else Key = Key.None;
            return keyType;
        }

        public void Clear() {
            Key = Key.None;
            Modifiers.Clear();
        }

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