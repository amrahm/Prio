using System;
using System.Windows.Input;
using Infrastructure.SharedResources;
using JetBrains.Annotations;
using NHotkey.Wpf;

namespace Prio.GlobalServices {
    [UsedImplicitly]
    public class PrioHotkeyManager : IPrioHotkeyManager {
        public void RegisterHotkey(Guid instanceId, string hotkeyName, ShortcutDefinition shortcut, Action action) {
            //HotkeyManager.Current.AddOrReplace("Increment", Key.Add, ModifierKeys.Control | ModifierKeys.Alt, action);

        }
    }
}