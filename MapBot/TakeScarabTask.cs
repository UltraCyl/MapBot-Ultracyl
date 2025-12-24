using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.Objects;
using StashUi = DreamPoeBot.Loki.Game.LokiPoe.InGameState.StashUi;

namespace Default.MapBot
{
    public class TakeScarabTask : ITask
    {
        private static readonly ScarabSettings Settings = ScarabSettings.Instance;
        private static bool _scarabsTaken;

        public async Task<bool> Run()
        {
            // Only run if scarabs are enabled
            if (!Settings.UseScarabs)
                return false;

            // Only run in hideout or town
            if (!World.CurrentArea.IsHideoutArea && !World.CurrentArea.IsTown)
                return false;

            // Only run if we haven't taken scarabs yet
            if (_scarabsTaken)
                return false;

            // Only run if we're about to open a map (OpenMapTask is enabled)
            if (!OpenMapTask.Enabled)
                return false;

            // Check which scarabs we need
            var neededScarabs = GetNeededScarabs();
            if (neededScarabs.Count == 0)
            {
                GlobalLog.Debug("[TakeScarabTask] No scarabs needed or all scarabs already in inventory.");
                _scarabsTaken = true;
                return false;
            }

            GlobalLog.Info($"[TakeScarabTask] Need to take {neededScarabs.Count} type(s) of scarabs from stash.");

            // Open stash
            if (!await Inventories.OpenStash())
            {
                ErrorManager.ReportError();
                return true;
            }

            // Try to find and take scarabs from fragment stash tab
            var fragmentTabName = GetFragmentTabName();
            if (fragmentTabName == null)
            {
                GlobalLog.Warn("[TakeScarabTask] No fragment stash tab found. Looking in regular tabs...");
                // Try regular stash tabs
                await TakeScarabsFromRegularTabs(neededScarabs);
            }
            else
            {
                // Open fragment tab and take scarabs
                if (!await Inventories.OpenStashTab(fragmentTabName))
                {
                    GlobalLog.Error($"[TakeScarabTask] Failed to open fragment tab: {fragmentTabName}");
                    ErrorManager.ReportError();
                    return true;
                }

                await TakeScarabsFromFragmentTab(neededScarabs);
            }

            _scarabsTaken = true;
            return true;
        }

        private static Dictionary<string, int> GetNeededScarabs()
        {
            var needed = new Dictionary<string, int>();
            var selectedScarabs = Settings.SelectedScarabs;

            // Count how many of each scarab we need
            foreach (var scarab in selectedScarabs)
            {
                if (scarab == "None") continue;

                if (!needed.ContainsKey(scarab))
                    needed[scarab] = 0;
                needed[scarab]++;
            }

            // Check limits and adjust
            foreach (var kvp in needed.ToList())
            {
                int limit = ScarabSettings.GetScarabLimit(kvp.Key);
                if (kvp.Value > limit)
                {
                    GlobalLog.Warn($"[TakeScarabTask] {kvp.Key} has limit of {limit}, but {kvp.Value} selected. Reducing to {limit}.");
                    needed[kvp.Key] = limit;
                }
            }

            // Subtract scarabs already in inventory
            foreach (var kvp in needed.ToList())
            {
                int inInventory = Inventories.InventoryItems.Count(i => i.Name == kvp.Key);
                int stillNeeded = kvp.Value - inInventory;

                if (stillNeeded <= 0)
                {
                    needed.Remove(kvp.Key);
                    GlobalLog.Debug($"[TakeScarabTask] Already have enough {kvp.Key} in inventory.");
                }
                else
                {
                    needed[kvp.Key] = stillNeeded;
                }
            }

            return needed;
        }

        private static string GetFragmentTabName()
        {
            var tabNames = StashUi.TabControl.TabNames;
            
            // Look for fragment tab by checking tab types
            foreach (var tabName in tabNames)
            {
                // Fragment tabs often have names like "Fragment", "Fragments", "Scarabs", etc.
                var lowerName = tabName.ToLower();
                if (lowerName.Contains("fragment") || lowerName.Contains("scarab"))
                {
                    return tabName;
                }
            }

            // Also check for the tab configured in settings
            var fragmentTabs = EXtensions.Settings.Instance.GetTabsForCategory(EXtensions.Settings.StashingCategory.Fragment);
            if (fragmentTabs.Count > 0)
            {
                return fragmentTabs[0];
            }

            return null;
        }

        private static async Task TakeScarabsFromFragmentTab(Dictionary<string, int> neededScarabs)
        {
            var tabType = StashUi.StashTabInfo?.TabType;

            if (tabType == InventoryTabType.Fragment)
            {
                GlobalLog.Debug("[TakeScarabTask] Fragment tab detected, using premium fragment tab API.");
                
                // For premium fragment tab, we need to iterate through all controls
                // Unfortunately, DreamPoeBot API for fragment tab scarab section may vary
                // We'll try to find items by searching all inventory controls
                
                foreach (var kvp in neededScarabs.ToList())
                {
                    string scarabName = kvp.Key;
                    int countNeeded = kvp.Value;

                    GlobalLog.Debug($"[TakeScarabTask] Looking for {countNeeded}x {scarabName} in fragment tab.");

                    // Try to get the inventory control for this scarab
                    // The fragment tab organizes items by metadata/type
                    var allItems = GetAllItemsFromFragmentTab();
                    
                    foreach (var item in allItems)
                    {
                        if (item.Name == scarabName && countNeeded > 0)
                        {
                            int toTake = System.Math.Min(countNeeded, item.StackCount);
                            GlobalLog.Info($"[TakeScarabTask] Taking {toTake}x {scarabName} from fragment tab.");

                            if (await FastMoveFromFragmentTab(item, toTake))
                            {
                                countNeeded -= toTake;
                                neededScarabs[scarabName] = countNeeded;
                            }

                            await Wait.SleepSafe(200);
                        }

                        if (countNeeded <= 0) break;
                    }

                    if (countNeeded > 0)
                    {
                        GlobalLog.Warn($"[TakeScarabTask] Could not find enough {scarabName}. Still need {countNeeded}.");
                    }
                }
            }
            else
            {
                // Regular stash tab
                await TakeScarabsFromRegularTab(neededScarabs);
            }
        }

        private static List<Item> GetAllItemsFromFragmentTab()
        {
            var items = new List<Item>();

            try
            {
                // Get all items from the current stash tab
                // For fragment tabs, Inventories.StashTabItems should work
                var stashItems = Inventories.StashTabItems;
                items.AddRange(stashItems);
            }
            catch (System.Exception ex)
            {
                GlobalLog.Error($"[TakeScarabTask] Error getting items from fragment tab: {ex.Message}");
            }

            return items;
        }

        private static async Task<bool> FastMoveFromFragmentTab(Item item, int count)
        {
            try
            {
                // For fragment tab, use the standard fast move
                var moved = StashUi.InventoryControl.FastMove(item.LocalId);
                if (moved != FastMoveResult.None)
                {
                    GlobalLog.Error($"[TakeScarabTask] Fast move error: {moved}");
                    return false;
                }

                await Wait.SleepSafe(100);
                return true;
            }
            catch (System.Exception ex)
            {
                GlobalLog.Error($"[TakeScarabTask] Error moving item: {ex.Message}");
                return false;
            }
        }

        private static async Task TakeScarabsFromRegularTabs(Dictionary<string, int> neededScarabs)
        {
            // Get all stash tab names
            var tabNames = StashUi.TabControl.TabNames;

            foreach (var tabName in tabNames)
            {
                if (neededScarabs.Count == 0) break;

                if (!await Inventories.OpenStashTab(tabName))
                    continue;

                await TakeScarabsFromRegularTab(neededScarabs);
            }
        }

        private static async Task TakeScarabsFromRegularTab(Dictionary<string, int> neededScarabs)
        {
            var stashItems = Inventories.StashTabItems.ToList();

            foreach (var kvp in neededScarabs.ToList())
            {
                string scarabName = kvp.Key;
                int countNeeded = kvp.Value;

                var matchingItems = stashItems.Where(i => i.Name == scarabName).ToList();
                
                foreach (var item in matchingItems)
                {
                    if (countNeeded <= 0) break;

                    int toTake = System.Math.Min(countNeeded, item.StackCount);
                    GlobalLog.Info($"[TakeScarabTask] Taking {toTake}x {scarabName} from stash.");

                    var moved = StashUi.InventoryControl.FastMove(item.LocalId);
                    if (moved == FastMoveResult.None)
                    {
                        countNeeded -= toTake;
                        neededScarabs[scarabName] = countNeeded;
                    }
                    else
                    {
                        GlobalLog.Error($"[TakeScarabTask] Fast move error: {moved}");
                    }

                    await Wait.SleepSafe(200);
                }

                if (countNeeded <= 0)
                {
                    neededScarabs.Remove(scarabName);
                }
            }
        }

        public MessageResult Message(Message message)
        {
            var id = message.Id;

            // Reset when a new map is entered
            if (id == MapBot.Messages.NewMapEntered)
            {
                _scarabsTaken = false;
                return MessageResult.Processed;
            }

            // Reset when map run is complete
            if (id == "ResetScarabTask")
            {
                _scarabsTaken = false;
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
            _scarabsTaken = false;
        }

        public void Stop()
        {
        }

        public string Name => "TakeScarabTask";
        public string Description => "Task for taking scarabs from stash before opening maps.";
        public string Author => "MapBot";
        public string Version => "1.0";

        #endregion
    }
}
