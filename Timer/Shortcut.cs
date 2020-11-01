using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;

namespace Timer {
    public class Shortcut {
        public enum KeyType { General, Alt, Ctrl, Shift, Win }


        private static readonly Dictionary<Key, KeyType> ModMap = new Dictionary<Key, KeyType> {
            {Key.LeftAlt, KeyType.Alt}, {Key.RightAlt, KeyType.Alt},
            {Key.LeftCtrl, KeyType.Ctrl}, {Key.RightCtrl, KeyType.Ctrl},
            {Key.LeftShift, KeyType.Shift}, {Key.RightShift, KeyType.Shift},
            {Key.LWin, KeyType.Win},  {Key.RWin, KeyType.Win}
        };


        public Key key;
        private readonly ISet<KeyType> _modifiers = new SortedSet<KeyType>();

        public KeyType AddKey(Key newKey) {
            if(ModMap.TryGetValue(newKey, out KeyType keyType)) {
                _modifiers.Add(keyType);
            } else key = newKey;
            return keyType;
        }

        public void AddModifier(KeyType newModifier) => _modifiers.Add(newModifier);
        public void RemoveModifier(KeyType newModifier) => _modifiers.Remove(newModifier);

        public KeyType RemoveKey(Key newKey) {
            if(ModMap.TryGetValue(newKey, out KeyType keyType)) {
                _modifiers.Remove(keyType);
            } else key = Key.None;
            return keyType;
        }

        public void Clear() {
            key = Key.None;
            _modifiers.Clear();
        }

        private static readonly KeyConverter  KeyConverter = new KeyConverter();

        public override string ToString() {
            string keyString = KeyConverter.ConvertToString(key) ?? string.Empty;
            keyString = keyString.Contains("Oem") ? GetCharFromKey(key).ToString().ToUpper() : keyString;
            return $"{string.Join("+", _modifiers)}{(_modifiers.Count == 0 ? "" : "+")}{keyString}";
        }


        private enum MapType : uint { MapvkVkToVsc = 0x0 }

        [DllImport("user32.dll")]
        private static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)] StringBuilder pwszBuff, int cchBuff, uint wFlags);


        [DllImport("user32.dll")] private static extern uint MapVirtualKey(uint uCode, MapType uMapType);

        private static char GetCharFromKey(Key key) {
            char ch = ' ';

            int virtualKey = KeyInterop.VirtualKeyFromKey(key);

            uint scanCode = MapVirtualKey((uint) virtualKey, MapType.MapvkVkToVsc);
            StringBuilder stringBuilder = new StringBuilder(2);

            int result = ToUnicode((uint) virtualKey, scanCode, new byte[256], stringBuilder, stringBuilder.Capacity, 0);
            switch(result) {
                case -1:
                case 0:
                    break;
                default: {
                    ch = stringBuilder[0];
                    break;
                }
            }
            return ch;
        }
    }
}