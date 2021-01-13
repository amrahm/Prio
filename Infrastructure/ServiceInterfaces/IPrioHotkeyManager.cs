using System;
using System.Collections.Generic;
using System.Linq;

namespace Prio.GlobalServices {
    public interface IPrioHotkeyManager {
        /// <summary> Registers an action to be called when a hotkey is pressed </summary>
        /// <param name="instanceId"> The instance ID of the registree </param>
        /// <param name="source"> The object holding the shortcut definition </param>
        /// <param name="sourcePropName"> The property name of the shortcut definition </param>
        /// <param name="action"> The action to be called </param>
        /// <param name="compatibilityType"> The type of hotkey to ensure only compatible types are registered to the same shortcut </param>
        /// <param name="instanceState"> What should the current instance state be for this to be called if multiple
        ///                              actions have the same shortcut for this instance</param>
        /// <param name="nextInstanceState"> Function to get the instance state action that should be called.
        ///                                  Input is the requested instanceState</param>
        /// <returns> true if the hotkey was able to be registered </returns>
        bool RegisterHotkey(Guid instanceId, object source, string sourcePropName, Action action,
                            CompatibilityType compatibilityType, int instanceState, Func<int, int> nextInstanceState);


        /// <summary> Registers an action to be called when a hotkey is pressed </summary>
        /// <param name="instanceId"> The instance ID of the registree </param>
        /// <param name="source"> The object holding the shortcut definition </param>
        /// <param name="sourcePropName"> The property name of the shortcut definition </param>
        /// <param name="action"> The action to be called </param>
        /// <param name="compatibilityType"> The type of hotkey to ensure only compatible types are registered to the same shortcut </param>
        /// <returns> true if the hotkey was able to be registered </returns>
        bool RegisterHotkey(Guid instanceId, object source, string sourcePropName, Action action,
                            CompatibilityType compatibilityType) =>
                RegisterHotkey(instanceId, source, sourcePropName, action, compatibilityType, -1, null);

        /// <summary> Registers an action to be called when a hotkey is pressed </summary>
        /// <param name="source"> The object holding the shortcut definition </param>
        /// <param name="sourcePropName"> The property name of the shortcut definition </param>
        /// <param name="action"> The action to be called </param>
        /// <param name="compatibilityType"> The type of hotkey to ensure only compatible types are registered to the same shortcut </param>
        /// <returns> true if the hotkey was able to be registered </returns>
        bool RegisterHotkey(object source, string sourcePropName, Action action,
                            CompatibilityType compatibilityType) =>
                RegisterHotkey(Guid.Empty, source, sourcePropName, action, compatibilityType);

        /// <summary> Registers an action to be called when a hotkey is pressed </summary>
        /// <param name="source"> The object holding the shortcut definition </param>
        /// <param name="sourcePropName"> The property name of the shortcut definition </param>
        /// <param name="action"> The action to be called </param>
        /// <param name="compatibilityType"> The type of hotkey to ensure only compatible types are registered to the same shortcut </param>
        /// <param name="instanceState"> What should the current instance state be for this to be called if multiple
        ///                              actions have the same shortcut for this instance</param>
        /// <param name="nextInstanceState"> Function to get the instance state action that should be called.
        ///                                  Input is the requested instanceState</param>
        /// <returns> true if the hotkey was able to be registered </returns>
        bool RegisterHotkey(object source, string sourcePropName, Action action,
                            CompatibilityType compatibilityType, int instanceState, Func<int, int> nextInstanceState) =>
                RegisterHotkey(Guid.Empty, source, sourcePropName, action, compatibilityType, instanceState,
                               nextInstanceState);

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