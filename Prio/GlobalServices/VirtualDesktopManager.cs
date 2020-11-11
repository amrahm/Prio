using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using WindowsDesktop;
using JetBrains.Annotations;

namespace Prio.GlobalServices {
    [UsedImplicitly]
    class VirtualDesktopManager : IVirtualDesktopManager {
        private VirtualDesktop[] _desktops;
        private Dictionary<Guid, int> _desktopMap;
        public event EventHandler<DesktopChangedEventArgs> DesktopChanged;

        public VirtualDesktopManager() {
            void RebuildDesktopsMap() {
                _desktops = VirtualDesktop.GetDesktops();
                _desktopMap = new Dictionary<Guid, int>();
                for(int i = 0; i < _desktops.Length; i++) _desktopMap.Add(_desktops[i].Id, i);
            }

            try {
                InitializeComObjects();
                RebuildDesktopsMap();

                VirtualDesktop.CurrentChanged += (o,  e) =>
                    DesktopChanged?.Invoke(
                        o, new DesktopChangedEventArgs(_desktopMap[e.OldDesktop.Id], _desktopMap[e.NewDesktop.Id]));

                VirtualDesktop.Created += (o,  e) => RebuildDesktopsMap();
                VirtualDesktop.Destroyed += (o,  e) => RebuildDesktopsMap();
            } catch(Exception e) {
                MessageBox.Show(e.Message, "Failed to initialize.");
            }
        }

        private static async void InitializeComObjects() =>
            await VirtualDesktopProvider.Default.Initialize(TaskScheduler.FromCurrentSynchronizationContext());

        public void MoveToDesktop(Window window, int desktopNum) =>
            window?.Dispatcher.Invoke(() => window.MoveToDesktop(_desktops[desktopNum]));

        public int CurrentDesktop() => _desktopMap[VirtualDesktop.Current.Id];
    }
}