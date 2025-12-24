using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Game.Objects;

namespace Default.MapBot
{
    public class OpenMapTask : ITask
    {
        internal static bool Enabled;

        public async Task<bool> Run()
        {
            if (!Enabled)
                return false;

            var area = World.CurrentArea;

            // This is more complicated after 3.0 because GGG added stairs to laboratory
            //if (!area.IsHideoutArea && !area.IsMapRoom)
            //    return false;

            if (area.IsHideoutArea)
                goto inProperArea;

            if (area.IsMapRoom)
            {
                if (await DeviceAreaTask.HandleStairs(true))
                    return true;

                goto inProperArea;
            }

            return false;

            inProperArea:
            var map = Inventories.InventoryItems.Find(i => i.IsMap());
            if (map == null)
            {
                GlobalLog.Error("[OpenMapTask] There is no map in inventory.");
                Enabled = false;
                return true;
            }

            var mapPos = map.LocationTopLeft;

            if (!await PlayerAction.TryTo(OpenDevice, "Open Map Device", 3, 2000))
            {
                ErrorManager.ReportError();
                return true;
            }
            if (!await ClearDevice())
            {
                ErrorManager.ReportError();
                return true;
            }
            if (!await PlayerAction.TryTo(() => PlaceIntoDevice(mapPos), "Place map into device", 3))
            {
                ErrorManager.ReportError();
                return true;
            }

            // Add scarabs if enabled
            if (ScarabSettings.Instance.UseScarabs)
            {
                await AddScarabsToDevice();
            }

            var fragment = Inventories.InventoryItems.Find(i => i.IsSacrificeFragment());
            if (fragment != null)
            {
                await PlayerAction.TryTo(() => PlaceIntoDevice(fragment.LocationTopLeft), "Place vaal fragment into device", 3);
            }

            // Check for existing portal (might exist from previous run)
            var existingPortal = LokiPoe.ObjectManager.Objects.Closest<Portal>();
            var hadExistingPortal = existingPortal != null && existingPortal.IsTargetable;

            // Activate the map device
            if (!await PlayerAction.TryTo(ActivateDevice, "Activate Map Device", 3))
            {
                ErrorManager.ReportError();
                return true;
            }
            
            // If there was an existing portal, wait for it to despawn
            if (hadExistingPortal && existingPortal != null)
            {
                if (!await Wait.For(() => !existingPortal.Fresh().IsTargetable, "old map portals despawning", 200, 10000))
                {
                    GlobalLog.Warn("[OpenMapTask] Old portals didn't despawn, continuing anyway.");
                }
            }
            
            // Wait for new map portals to spawn
            if (!await Wait.For(() =>
                {
                    var p = LokiPoe.ObjectManager.Objects.Closest<Portal>();
                    return p != null && p.IsTargetable && p.LeadsTo(a => a.IsMap);
                },
                "new map portals spawning", 500, 15000))
            {
                GlobalLog.Error("[OpenMapTask] Failed to find map portals after activating device.");
                ErrorManager.ReportError();
                return true;
            }

            // Mark that we're on a run now - portals are open
            MapBot.IsOnRun = true;

            await Wait.SleepSafe(500);

            var portal = LokiPoe.ObjectManager.Objects.Closest<Portal>(p => p.IsTargetable && p.LeadsTo(a => a.IsMap));
            if (portal == null)
            {
                GlobalLog.Error("[OpenMapTask] Portals spawned but now can't find them.");
                ErrorManager.ReportError();
                return true;
            }

            if (!await TakeMapPortal(portal))
                ErrorManager.ReportError();

            return true;
        }

        private static async Task<bool> OpenDevice()
        {
            if (MapDevice.IsOpen) return true;

            var device = LokiPoe.ObjectManager.MapDevice;
            if (device == null)
            {
                if (World.CurrentArea.IsHideoutArea)
                {
                    GlobalLog.Error("[OpenMapTask] Fail to find Map Device in hideout.");
                }
                else
                {
                    GlobalLog.Error("[OpenMapTask] Unknown error. Fail to find Map Device in Templar Laboratory.");
                }
                GlobalLog.Error("[OpenMapTask] Now stopping the bot because it cannot continue.");
                BotManager.Stop();
                return false;
            }

            GlobalLog.Debug("[OpenMapTask] Now going to open Map Device.");

            await device.WalkablePosition().ComeAtOnce();

            if (await PlayerAction.Interact(device, () => MapDevice.IsOpen, "Map Device opening"))
            {
                GlobalLog.Debug("[OpenMapTask] Map Device has been successfully opened.");
                return true;
            }
            GlobalLog.Debug("[OpenMapTask] Fail to open Map Device.");
            return false;
        }

        private static async Task<bool> ClearDevice()
        {
            var itemPositions = MapDevice.InventoryControl.Inventory.Items.Select(i => i.LocationTopLeft).ToList();
            if (itemPositions.Count == 0)
                return true;

            GlobalLog.Error("[OpenMapTask] Map Device is not empty. Now going to clean it.");

            foreach (var itemPos in itemPositions)
            {
                if (!await PlayerAction.TryTo(() => FastMoveFromDevice(itemPos), null, 2))
                    return false;
            }
            GlobalLog.Debug("[OpenMapTask] Map Device has been successfully cleaned.");
            return true;
        }

        private static async Task<bool> PlaceIntoDevice(Vector2i itemPos)
        {
            var oldCount = MapDevice.InventoryControl.Inventory.Items.Count;

            if (!await Inventories.FastMoveFromInventory(itemPos))
                return false;

            if (!await Wait.For(() => MapDevice.InventoryControl.Inventory.Items.Count == oldCount + 1, "item amount change in Map Device"))
                return false;

            return true;
        }

        private static async Task<bool> ActivateDevice()
        {
            GlobalLog.Debug("[OpenMapTask] Now going to activate the Map Device.");

            await Wait.SleepSafe(500); // Additional delay to ensure Activate button is targetable

            var map = MapDevice.InventoryControl.Inventory.Items.Find(i=> i.Class == ItemClasses.Map);
            if (map == null)
            {
                GlobalLog.Error("[OpenMapTask] Unexpected error. There is no map in the Map Device.");
                return false;
            }

            LokiPoe.InGameState.ActivateResult activated;

            if (World.CurrentArea.IsHideoutArea)
            {
                // Simply activate the map device in hideout
                activated = LokiPoe.InGameState.MasterDeviceUi.Activate();
            }
            else
            {
                activated = LokiPoe.InGameState.MapDeviceUi.Activate();
            }

            if (activated != LokiPoe.InGameState.ActivateResult.None)
            {
                GlobalLog.Error($"[OpenMapTask] Fail to activate the Map Device. Error: \"{activated}\".");
                return false;
            }
            if (await Wait.For(() => !MapDevice.IsOpen, "Map Device closing"))
            {
                GlobalLog.Debug("[OpenMapTask] Map Device has been successfully activated.");
                return true;
            }
            GlobalLog.Error("[OpenMapTask] Fail to activate the Map Device.");
            return false;
        }

        private static async Task<bool> FastMoveFromDevice(Vector2i itemPos)
        {
            var item = MapDevice.InventoryControl.Inventory.FindItemByPos(itemPos);
            if (item == null)
            {
                GlobalLog.Error($"[FastMoveFromDevice] Fail to find item at {itemPos} in Map Device.");
                return false;
            }

            var itemName = item.FullName;

            GlobalLog.Debug($"[FastMoveFromDevice] Fast moving \"{itemName}\" at {itemPos} from Map Device.");

            var moved = MapDevice.InventoryControl.FastMove(item.LocalId);
            if (moved != FastMoveResult.None)
            {
                GlobalLog.Error($"[FastMoveFromDevice] Fast move error: \"{moved}\".");
                return false;
            }
            if (await Wait.For(() => MapDevice.InventoryControl.Inventory.FindItemByPos(itemPos) == null, "fast move"))
            {
                GlobalLog.Debug($"[FastMoveFromDevice] \"{itemName}\" at {itemPos} has been successfully fast moved from Map Device.");
                return true;
            }
            GlobalLog.Error($"[FastMoveFromDevice] Fast move timeout for \"{itemName}\" at {itemPos} in Map Device.");
            return false;
        }

        private static async Task<bool> TakeMapPortal(Portal portal, int attempts = 3)
        {
            for (int i = 1; i <= attempts; ++i)
            {
                if (!LokiPoe.IsInGame || World.CurrentArea.IsMap)
                    return true;

                GlobalLog.Debug($"[OpenMapTask] Take portal to map attempt: {i}/{attempts}");

                if (await PlayerAction.TakePortal(portal))
                    return true;

                await Wait.SleepSafe(1000);
            }
            return false;
        }

        private static async Task AddScarabsToDevice()
        {
            var settings = ScarabSettings.Instance;
            var selectedScarabs = settings.SelectedScarabs;

            if (selectedScarabs.Count == 0)
            {
                GlobalLog.Debug("[OpenMapTask] No scarabs selected.");
                return;
            }

            GlobalLog.Info($"[OpenMapTask] Adding {selectedScarabs.Count} scarab(s) to map device.");

            // Count how many of each scarab we need
            var scarabCounts = new System.Collections.Generic.Dictionary<string, int>();
            foreach (var scarab in selectedScarabs)
            {
                if (!scarabCounts.ContainsKey(scarab))
                    scarabCounts[scarab] = 0;
                scarabCounts[scarab]++;
            }

            // Check limits and adjust counts
            foreach (var kvp in scarabCounts.ToList())
            {
                int limit = ScarabSettings.GetScarabLimit(kvp.Key);
                if (kvp.Value > limit)
                {
                    GlobalLog.Warn($"[OpenMapTask] {kvp.Key} has limit of {limit}, but {kvp.Value} selected. Reducing to {limit}.");
                    scarabCounts[kvp.Key] = limit;
                }
            }

            // Find and add scarabs from inventory
            foreach (var kvp in scarabCounts)
            {
                string scarabName = kvp.Key;
                int countNeeded = kvp.Value;

                for (int i = 0; i < countNeeded; i++)
                {
                    var scarab = Inventories.InventoryItems.Find(item => item.Name == scarabName);
                    if (scarab == null)
                    {
                        GlobalLog.Warn($"[OpenMapTask] Could not find {scarabName} in inventory. Skipping.");
                        continue;
                    }

                    GlobalLog.Debug($"[OpenMapTask] Adding {scarabName} to map device.");

                    if (!await PlayerAction.TryTo(() => PlaceIntoDevice(scarab.LocationTopLeft), $"Place {scarabName} into device", 3))
                    {
                        GlobalLog.Error($"[OpenMapTask] Failed to add {scarabName} to map device.");
                        continue;
                    }

                    await Wait.SleepSafe(200);
                }
            }

            GlobalLog.Info("[OpenMapTask] Finished adding scarabs.");
        }

        public MessageResult Message(Message message)
        {
            if (message.Id == MapBot.Messages.NewMapEntered)
            {
                Enabled = false;
                return MessageResult.Processed;
            }
            return MessageResult.Unprocessed;
        }

        private static class MapDevice
        {
            public static bool IsOpen => World.CurrentArea.IsHideoutArea
                ? LokiPoe.InGameState.MasterDeviceUi.IsOpened
                : LokiPoe.InGameState.MapDeviceUi.IsOpened;

            public static InventoryControlWrapper InventoryControl => World.CurrentArea.IsHideoutArea
                ? LokiPoe.InGameState.MasterDeviceUi.InventoryControl
                : LokiPoe.InGameState.MapDeviceUi.InventoryControl;
        }

        #region Unused interface methods

        public async Task<LogicResult> Logic(Logic logic)
        {
            return LogicResult.Unprovided;
        }

        public void Tick()
        {
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public string Name => "OpenMapTask";
        public string Description => "Task for opening maps via Map Device.";
        public string Author => "ExVault";
        public string Version => "1.0";

        #endregion
    }
}