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
                if (lowerName.Contains("fragment") || lowerName.Contains("scarab") || lowerName.Contains("frag"))
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
                GlobalLog.Debug("[TakeScarabTask] Fragment tab detected, navigating to Scarab sub-section.");
                
                // For premium fragment tab, we need to:
                // 1. Get the inventory control for scarabs using metadata
                // 2. Or click on the Scarab sub-tab
                
                // First, try to get all scarab items using metadata matching
                // Scarab metadata is typically: Metadata/Items/Scarabs/*
                
                foreach (var kvp in neededScarabs.ToList())
                {
                    string scarabName = kvp.Key;
                    int countNeeded = kvp.Value;

                    GlobalLog.Debug($"[TakeScarabTask] Looking for {countNeeded}x {scarabName} in fragment tab.");

                    // Try to find the scarab using the FragmentTab's GetInventoryControlForMetadata
                    // First we need to find any scarab to get the metadata pattern
                    bool found = false;
                    
                    // Try to find using different possible metadata patterns for scarabs
                    var possibleMetadatas = new List<string>
                    {
                        $"Metadata/Items/Scarabs/{scarabName.Replace(" ", "")}",
                        $"Metadata/Items/Scarabs/{scarabName.Replace(" Scarab", "").Replace(" ", "")}Scarab",
                    };
                    
                    // Also try to iterate through all inventory controls in the fragment tab
                    try
                    {
                        // Get all inventory controls from FragmentTab and search for scarabs
                        // The FragmentTab has multiple sub-inventories for different sections
                        
                        // Try to get items by iterating through the fragment tab's inventory
                        var allItems = GetAllScarabsFromFragmentTab();
                        
                        foreach (var item in allItems)
                        {
                            if (item.Name == scarabName && countNeeded > 0)
                            {
                                int toTake = System.Math.Min(countNeeded, item.StackCount);
                                GlobalLog.Info($"[TakeScarabTask] Found {item.Name} (Stack: {item.StackCount}). Taking {toTake}.");

                                // Get the control for this specific item
                                var control = StashUi.FragmentTab.GetInventoryControlForMetadata(item.Metadata);
                                if (control != null)
                                {
                                    var moved = control.FastMove(item.LocalId);
                                    if (moved == FastMoveResult.None)
                                    {
                                        countNeeded -= toTake;
                                        neededScarabs[scarabName] = countNeeded;
                                        found = true;
                                        GlobalLog.Debug($"[TakeScarabTask] Successfully moved {item.Name}.");
                                    }
                                    else
                                    {
                                        GlobalLog.Error($"[TakeScarabTask] Fast move error: {moved}");
                                    }
                                }
                                else
                                {
                                    // Try using the standard stash inventory control
                                    var moved = StashUi.InventoryControl.FastMove(item.LocalId);
                                    if (moved == FastMoveResult.None)
                                    {
                                        countNeeded -= toTake;
                                        neededScarabs[scarabName] = countNeeded;
                                        found = true;
                                        GlobalLog.Debug($"[TakeScarabTask] Successfully moved {item.Name} using standard control.");
                                    }
                                    else
                                    {
                                        GlobalLog.Error($"[TakeScarabTask] Fast move error: {moved}");
                                    }
                                }

                                await Wait.SleepSafe(200);
                            }

                            if (countNeeded <= 0) break;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        GlobalLog.Error($"[TakeScarabTask] Error accessing fragment tab: {ex.Message}");
                    }

                    if (!found && countNeeded > 0)
                    {
                        GlobalLog.Warn($"[TakeScarabTask] Could not find {scarabName} in fragment tab. Still need {countNeeded}.");
                    }
                }
            }
            else
            {
                GlobalLog.Debug($"[TakeScarabTask] Tab is not a Fragment tab (type: {tabType}). Using regular tab method.");
                // Regular stash tab
                await TakeScarabsFromRegularTab(neededScarabs);
            }
        }

        private static List<Item> GetAllScarabsFromFragmentTab()
        {
            var items = new List<Item>();

            try
            {
                // The FragmentTab has multiple sub-inventories
                // We need to check each one for scarabs
                
                var fragmentTab = StashUi.FragmentTab;
                if (fragmentTab == null)
                {
                    GlobalLog.Error("[TakeScarabTask] FragmentTab is null.");
                    return items;
                }
                
                // Try to get all inventory controls from the fragment tab
                // FragmentTab typically has: General, Scarab, Breach, Betrayal, Eldritch sections
                
                // Get the current items visible in the stash
                var stashItems = Inventories.StashTabItems;
                
                // Filter for scarabs (items with "Scarab" in name)
                foreach (var item in stashItems)
                {
                    if (item.Name != null && item.Name.Contains("Scarab"))
                    {
                        items.Add(item);
                        GlobalLog.Debug($"[TakeScarabTask] Found scarab in stash: {item.Name} (Stack: {item.StackCount})");
                    }
                }
                
                // If no scarabs found, we may need to switch sub-tabs
                if (items.Count == 0)
                {
                    GlobalLog.Warn("[TakeScarabTask] No scarabs visible in current fragment tab view. May need to switch to Scarab sub-tab.");
                    
                    // Try to switch to Scarab sub-tab by clicking on it
                    // The sub-tab buttons are typically in a specific UI element
                    // We need to use the FragmentTab's specific API or click coordinates
                    
                    // Try to use SwitchToSubTab if available
                    TrySwitchToScarabSubTab();
                    
                    // Re-scan after switching
                    await Wait.SleepSafe(300);
                    stashItems = Inventories.StashTabItems;
                    foreach (var item in stashItems)
                    {
                        if (item.Name != null && item.Name.Contains("Scarab"))
                        {
                            items.Add(item);
                            GlobalLog.Debug($"[TakeScarabTask] Found scarab after sub-tab switch: {item.Name} (Stack: {item.StackCount})");
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                GlobalLog.Error($"[TakeScarabTask] Error getting scarabs from fragment tab: {ex.Message}");
            }

            return items;
        }
        
        private static void TrySwitchToScarabSubTab()
        {
            try
            {
                // The Fragment stash tab has sub-tabs that can be accessed via UI
                // In DreamPoeBot, we may need to:
                // 1. Use a specific API call if available
                // 2. Or click on the sub-tab button
                
                var fragmentTab = StashUi.FragmentTab;
                if (fragmentTab == null)
                {
                    GlobalLog.Error("[TakeScarabTask] Cannot switch sub-tab: FragmentTab is null.");
                    return;
                }
                
                // Try to find and use ViewScarabs or similar method
                // Check if there's a method to switch views
                var fragmentTabType = fragmentTab.GetType();
                
                // Log available methods for debugging
                GlobalLog.Debug($"[TakeScarabTask] FragmentTab type: {fragmentTabType.Name}");
                
                // Try reflection to find sub-tab switching methods
                var methods = fragmentTabType.GetMethods();
                foreach (var method in methods)
                {
                    if (method.Name.ToLower().Contains("scarab") || 
                        method.Name.ToLower().Contains("view") ||
                        method.Name.ToLower().Contains("switch") ||
                        method.Name.ToLower().Contains("tab"))
                    {
                        GlobalLog.Debug($"[TakeScarabTask] Found potentially useful method: {method.Name}");
                    }
                }
                
                // Try to find properties that might give us access to sub-tabs
                var properties = fragmentTabType.GetProperties();
                foreach (var prop in properties)
                {
                    if (prop.Name.ToLower().Contains("scarab") || 
                        prop.Name.ToLower().Contains("inventory") ||
                        prop.Name.ToLower().Contains("control"))
                    {
                        GlobalLog.Debug($"[TakeScarabTask] Found potentially useful property: {prop.Name}");
                    }
                }
                
                // If we can't find a programmatic way, we might need to click on the sub-tab
                // The Scarab sub-tab is typically the second button in the Fragment tab UI
                // Based on the screenshot, the sub-tabs are at the bottom of the stash
                
                // Try clicking on the Scarab sub-tab button
                // Approximate position based on typical UI layout
                GlobalLog.Info("[TakeScarabTask] Attempting to click on Scarab sub-tab...");
                
                // Use input simulation to click on the Scarab tab
                // The sub-tab buttons are typically in a row at the bottom of the fragment stash
                // Position varies based on resolution, but we can try to use relative positions
                
                // Get the stash panel bounds if available
                // For now, try a direct approach using known sub-tab index
                TryClickScarabSubTab();
            }
            catch (System.Exception ex)
            {
                GlobalLog.Error($"[TakeScarabTask] Error switching to Scarab sub-tab: {ex.Message}");
            }
        }
        
        private static void TryClickScarabSubTab()
        {
            try
            {
                // The Fragment tab has buttons for each sub-section
                // Based on the image: General, Scarab, Breach, Betrayal, Eldritch
                // Scarab is the second button (index 1)
                
                // We need to click on this button to switch to the Scarab view
                // Use LokiPoe's input system to click
                
                // Try to find the button element in the UI
                // The sub-tabs in fragment stash are typically accessed via specific UI elements
                
                // Attempt using keyboard navigation if possible (Tab or arrow keys)
                // Or use direct mouse click
                
                // For premium stash tabs, there might be a specific API
                // Let's try to use the StashUi API to switch sub-inventories
                
                // Get all inventory controls and find the one for scarabs
                var fragmentTab = StashUi.FragmentTab;
                
                // Try to access sub-inventories through reflection or known patterns
                // Scarab metadata typically starts with "Metadata/Items/Scarabs/"
                
                // Try to get a control for any scarab metadata
                var testMetadatas = new[]
                {
                    "Metadata/Items/Scarabs/ScarabBreachGlobal1",
                    "Metadata/Items/Scarabs/ScarabLegionGlobal1",
                    "Metadata/Items/Scarabs/Kalguuran",
                };
                
                foreach (var metadata in testMetadatas)
                {
                    var control = fragmentTab.GetInventoryControlForMetadata(metadata);
                    if (control != null)
                    {
                        GlobalLog.Debug($"[TakeScarabTask] Found inventory control for metadata: {metadata}");
                        // Getting the control should automatically navigate to that sub-tab
                        // Or at least give us access to the scarab inventory
                        break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                GlobalLog.Error($"[TakeScarabTask] Error clicking Scarab sub-tab: {ex.Message}");
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
