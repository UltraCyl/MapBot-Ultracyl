using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.Objects;

namespace Default.MapBot
{
    public class AddScarabTask : ITask
    {
        private static readonly ScarabSettings Settings = ScarabSettings.Instance;
        private static bool _scarabsAdded;

        public async Task<bool> Run()
        {
            // Only run if scarabs are enabled
            if (!Settings.UseScarabs)
                return false;

            // Only run in hideout
            if (!World.CurrentArea.IsHideoutArea)
                return false;

            // Only run if we haven't added scarabs yet for this map
            if (_scarabsAdded)
                return false;

            // Only run if OpenMapTask is enabled (meaning we're about to open a map)
            if (!OpenMapTask.Enabled)
                return false;

            // Check if map device is open
            if (!LokiPoe.InGameState.MasterDeviceUi.IsOpened)
                return false;

            var selectedScarabs = Settings.SelectedScarabs;
            if (selectedScarabs.Count == 0)
            {
                GlobalLog.Debug("[AddScarabTask] No scarabs selected.");
                _scarabsAdded = true;
                return false;
            }

            GlobalLog.Info($"[AddScarabTask] Attempting to add {selectedScarabs.Count} scarab(s) to map device.");

            // Count how many of each scarab we need
            var scarabCounts = new Dictionary<string, int>();
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
                    GlobalLog.Warn($"[AddScarabTask] {kvp.Key} has limit of {limit}, but {kvp.Value} selected. Reducing to {limit}.");
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
                        GlobalLog.Warn($"[AddScarabTask] Could not find {scarabName} in inventory. Skipping.");
                        continue;
                    }

                    GlobalLog.Debug($"[AddScarabTask] Adding {scarabName} to map device.");

                    if (!await PlaceScarabInDevice(scarab.LocationTopLeft))
                    {
                        GlobalLog.Error($"[AddScarabTask] Failed to add {scarabName} to map device.");
                        continue;
                    }

                    await Wait.SleepSafe(200);
                }
            }

            _scarabsAdded = true;
            GlobalLog.Info("[AddScarabTask] Finished adding scarabs.");
            return true;
        }

        private static async Task<bool> PlaceScarabInDevice(Vector2i itemPos)
        {
            var deviceControl = LokiPoe.InGameState.MasterDeviceUi.InventoryControl;
            if (deviceControl == null)
            {
                GlobalLog.Error("[AddScarabTask] Map device inventory control is null.");
                return false;
            }

            var oldCount = deviceControl.Inventory.Items.Count;

            if (!await Inventories.FastMoveFromInventory(itemPos))
                return false;

            if (!await Wait.For(() => deviceControl.Inventory.Items.Count == oldCount + 1, "scarab placed in device", 200, 3000))
                return false;

            return true;
        }

        public MessageResult Message(Message message)
        {
            var id = message.Id;

            // Reset when a new map is entered
            if (id == MapBot.Messages.NewMapEntered)
            {
                _scarabsAdded = false;
                return MessageResult.Processed;
            }

            // Reset when OpenMapTask is enabled (new map being opened)
            if (id == "ResetScarabTask")
            {
                _scarabsAdded = false;
                return MessageResult.Processed;
            }

            return MessageResult.Unprocessed;
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
            _scarabsAdded = false;
        }

        public void Stop()
        {
        }

        public string Name => "AddScarabTask";
        public string Description => "Task for adding scarabs to the map device.";
        public string Author => "MapBot";
        public string Version => "1.0";

        #endregion
    }
}
