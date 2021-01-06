using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Infrastructure.SharedResources;
using JetBrains.Annotations;
using NHotkey;
using NHotkey.Wpf;
using static Prio.GlobalServices.IPrioHotkeyManager;

namespace Prio.GlobalServices {
    [UsedImplicitly]
    public class PrioHotkeyManager : IPrioHotkeyManager {
        private static readonly Dictionary<ShortcutDefinition, HotkeyHolder> HotKeyRegistry =
                new();

        private static readonly Dictionary<HotkeyRegistration, ShortcutDefinition> RegistrationToShortcut =
                new();

        private class HotkeyHolder {
            private EventHandler<HotkeyEventArgs> _handler;

            private readonly Dictionary<HotkeyRegistration, EventHandler<HotkeyEventArgs>> _registrations =
                    new();

            private readonly ShortcutDefinition _shortcut;
            private readonly Queue<Action> _actionQueue = new();

            public HotkeyHolder(HotkeyRegistration registration, Action action) {
                _shortcut = registration.Shortcut;
                AddAction(registration, action);
            }

            /// <summary> Call everything in the action queue </summary>
            private void CallActionQueue(object sender, HotkeyEventArgs args) {
                while(_actionQueue.Count > 0) _actionQueue.Dequeue()();
            }

            /// <summary> Adds a new action to be called when this hotkey is pressed </summary>
            /// <param name="registration"> Info about who added this action </param>
            /// <param name="action"> The action to be called </param>
            /// <returns> true if the action was able to be added </returns>
            public bool AddAction(HotkeyRegistration registration, Action action) {
                Debug.Assert(Equals(registration.Shortcut, _shortcut),
                             $"Tried adding registration with different shortcut to {nameof(HotkeyHolder)}");
                if(!CheckCompatibilities(registration.CompatibilityType,
                                         _registrations.Keys.Select(x => x.CompatibilityType))) return false;

                // Add a handler to queue up all the actions to be taken
                // This lets us first check the instance state on all events before changing it with one of them
                EventHandler<HotkeyEventArgs> handler;
                if(registration.GetInstanceState == null)
                    handler = (_, _) => _actionQueue.Enqueue(action);
                else {
                    handler = (_, _) => {
                        if(registration.GetInstanceState(registration.InstanceState) == registration.InstanceState)
                            _actionQueue.Enqueue(action);
                    };
                }
                _registrations.Add(registration, handler);

                // Ensure that CallActionQueue is last, so that all the actions are queued up before firing
                _handler -= CallActionQueue;
                _handler += handler;
                _handler += CallActionQueue;

                HotkeyManager.Current.AddOrReplace(_shortcut.ToString(), _shortcut.Key, _shortcut.Modifiers, _handler);
                return true;
            }

            /// <summary> Remove the action associated with the registration if it exists </summary>
            public void RemoveAction(HotkeyRegistration registration) {
                if(_registrations.ContainsKey(registration)) {
                    _handler -= _registrations[registration];
                    _registrations.Remove(registration);
                    if(_registrations.Count == 0) {
                        _handler -= CallActionQueue;
                        HotkeyManager.Current.Remove(_shortcut.ToString());
                        HotKeyRegistry.Remove(_shortcut);
                    }
                }
            }
        }

        private class HotkeyRegistration {
            private Guid InstanceId { get; }
            private string HotkeyName { get; }
            public ShortcutDefinition Shortcut { get; }
            public CompatibilityType CompatibilityType { get; }
            public int InstanceState { get; }
            public Func<int, int> GetInstanceState { get; }

            public HotkeyRegistration(Guid instanceId, string hotkeyName, ShortcutDefinition shortcut = null,
                                      CompatibilityType compatibilityType = CompatibilityType.General, int instanceState = 0,
                                      Func<int, int> getInstanceState = null) {
                InstanceId = instanceId;
                HotkeyName = hotkeyName;
                Shortcut = shortcut;
                CompatibilityType = compatibilityType;
                InstanceState = instanceState;
                GetInstanceState = getInstanceState;
            }

            public override bool Equals(object obj) =>
                    obj is HotkeyRegistration other && InstanceId.Equals(other.InstanceId) && HotkeyName == other.HotkeyName;

            public override int GetHashCode() => HashCode.Combine(InstanceId, HotkeyName);
        }

        public bool RegisterHotkey(Guid instanceId, string hotkeyName, ShortcutDefinition shortcut, Action action,
                                   CompatibilityType compatibilityType, int instanceState,
                                   Func<int, int> nextInstanceState) {
            HotkeyRegistration registration = new(instanceId, hotkeyName, shortcut,
                                                  compatibilityType, instanceState, nextInstanceState);

            UnregisterHotkey(registration);

            if(shortcut == null || shortcut.Key == Key.None) return true; //If setting it to null, just unregister and return

            bool registeredHotkey = true;

            if(HotKeyRegistry.TryGetValue(shortcut, out HotkeyHolder holder))
                registeredHotkey = holder.AddAction(registration, action);
            else
                HotKeyRegistry[shortcut] = new HotkeyHolder(registration, action);

            if(registeredHotkey) RegistrationToShortcut[registration] = shortcut;
            return registeredHotkey;
        }


        public void UnregisterHotkey(Guid instanceId, string hotkeyName) =>
                UnregisterHotkey(new HotkeyRegistration(instanceId, hotkeyName));

        /// <summary> Remove the current registration if it exists </summary>
        private static void UnregisterHotkey(HotkeyRegistration registration) {
            if(RegistrationToShortcut.TryGetValue(registration, out ShortcutDefinition shortcut) &&
               HotKeyRegistry.TryGetValue(shortcut, out HotkeyHolder holder)) {
                holder.RemoveAction(registration);
                RegistrationToShortcut.Remove(registration);
            }
        }
    }
}