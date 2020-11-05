using System;
using Infrastructure.SharedResources;

namespace Prio.GlobalServices {
    public interface IPrioHotkeyManager {
        void RegisterHotkey(Guid instanceId, string hotkeyName, ShortcutDefinition shortcut, Action action);
    }
}