using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.SharedResources;

namespace Prio.GlobalServices {
    public interface IPrioHotkeyManager {
        /// <summary> Registers an action to be called when a hotkey is pressed </summary>
        /// <param name="instanceId"> The instance ID of the registree </param>
        /// <param name="hotkeyName"> The name of the hotkey </param>
        /// <param name="shortcut"> The shortcut to trigger the action </param>
        /// <param name="action"> The action to be called </param>
        /// <param name="compatibilityType"> The type of hotkey to ensure only compatible types are registered to the same shortcut </param>
        /// <param name="instanceState"> What should the current instance state be for this to be called if multiple
        ///                              actions have the same shortcut for this instance</param>
        /// <param name="getInstanceState"> Function to get the current instance state </param>
        /// <returns> true if the hotkey was able to be registered </returns>
        bool RegisterHotkey(Guid instanceId, string hotkeyName, ShortcutDefinition shortcut, Action action,
            CompatibilityType compatibilityType, int instanceState, Func<int> getInstanceState);


        /// <summary> Registers an action to be called when a hotkey is pressed </summary>
        /// <param name="instanceId"> The instance ID of the registree </param>
        /// <param name="hotkeyName"> The name of the hotkey </param>
        /// <param name="shortcut"> The shortcut to trigger the action </param>
        /// <param name="action"> The action to be called </param>
        /// <param name="compatibilityType"> The type of hotkey to ensure only compatible types are registered to the same shortcut </param>
        /// <returns> true if the hotkey was able to be registered </returns>
        bool RegisterHotkey(Guid instanceId, string hotkeyName, ShortcutDefinition shortcut, Action action,
            CompatibilityType compatibilityType) =>
            RegisterHotkey(instanceId, hotkeyName, shortcut, action, compatibilityType, -1, null);

        /// <summary> Registers an action to be called when a hotkey is pressed </summary>
        /// <param name="hotkeyName"> The name of the hotkey </param>
        /// <param name="shortcut"> The shortcut to trigger the action </param>
        /// <param name="action"> The action to be called </param>
        /// <param name="compatibilityType"> The type of hotkey to ensure only compatible types are registered to the same shortcut </param>
        /// <returns> true if the hotkey was able to be registered </returns>
        bool RegisterHotkey(string hotkeyName, ShortcutDefinition shortcut, Action action,
            CompatibilityType compatibilityType) =>
            RegisterHotkey(Guid.Empty, hotkeyName, shortcut, action, compatibilityType);

        /// <summary> Unregisters a hotkey if it exists </summary>
        /// <param name="instanceId"> The instance ID of the registree </param>
        /// <param name="hotkeyName"> The name of the hotkey </param>
        void UnregisterHotkey(Guid instanceId, string hotkeyName);

        /// <summary> Unregisters a hotkey if it exists </summary>
        /// <param name="hotkeyName"> The name of the hotkey </param>
        void UnregisterHotkey(string hotkeyName) => UnregisterHotkey(Guid.Empty, hotkeyName);


        static bool CheckCompatibilities(CompatibilityType newType, IEnumerable<CompatibilityType> existingTypes) =>
            existingTypes.Where(type => type != CompatibilityType.General).All(type => type == newType);
    }

    public enum CompatibilityType { General, GeneralExclusive, Reset, StartStop, Visibility }
}