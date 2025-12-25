using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Default.EXtensions;
using DreamPoeBot.Loki.Bot;
using DreamPoeBot.Loki.Game;
using DreamPoeBot.Loki.Game.Objects;
using StashUi = DreamPoeBot.Loki.Game.LokiPoe.InGameState.StashUi;
using FragmentTab = DreamPoeBot.Loki.Game.LokiPoe.InGameState.StashUi.FragmentTab;

namespace Default.MapBot
{
    public class TakeScarabTask : ITask
    {
        private static readonly ScarabSettings Settings = ScarabSettings.Instance;

        // Dictionary mapping scarab names to their metadata paths
        // Based on https://poedb.tw/us/Fragment_Stash_Tab#FragmentStashTab
        private static readonly Dictionary<string, string> ScarabMetadata = new Dictionary<string, string>
        {
            // Breach Scarabs
            {"Breach Scarab", "Metadata/Items/Scarabs/ScarabBreachGlobal1"},
            {"Breach Scarab of the Dreamer", "Metadata/Items/Scarabs/ScarabBreachGlobal4"},
            {"Breach Scarab of Lordship", "Metadata/Items/Scarabs/ScarabBreachGlobal2"},
            {"Breach Scarab of Splintering", "Metadata/Items/Scarabs/ScarabBreachGlobal3"},
            {"Breach Scarab of Snares", "Metadata/Items/Scarabs/ScarabBreachGlobal5"},
            {"Breach Scarab of Resonant Cascade", "Metadata/Items/Scarabs/ScarabBreachGlobal6"},
            
            // Legion Scarabs
            {"Legion Scarab", "Metadata/Items/Scarabs/ScarabLegionGlobal1"},
            {"Legion Scarab of Officers", "Metadata/Items/Scarabs/ScarabLegionGlobal2"},
            {"Legion Scarab of Command", "Metadata/Items/Scarabs/ScarabLegionGlobal3"},
            {"Legion Scarab of The Sekhema", "Metadata/Items/Scarabs/ScarabLegionGlobal4"},
            {"Legion Scarab of Eternal Conflict", "Metadata/Items/Scarabs/ScarabLegionGlobal5"},
            
            // Abyss Scarabs
            {"Abyss Scarab", "Metadata/Items/Scarabs/ScarabAbyssGlobal1"},
            {"Abyss Scarab of Multitudes", "Metadata/Items/Scarabs/ScarabAbyssGlobal2"},
            {"Abyss Scarab of Edifice", "Metadata/Items/Scarabs/ScarabAbyssGlobal3"},
            {"Abyss Scarab of Emptiness", "Metadata/Items/Scarabs/ScarabAbyssGlobal4"},
            {"Abyss Scarab of Profound Depth", "Metadata/Items/Scarabs/ScarabAbyssGlobal5"},
            
            // Harbinger Scarabs
            {"Harbinger Scarab", "Metadata/Items/Scarabs/ScarabHarbingerGlobal1"},
            {"Harbinger Scarab of Obelisks", "Metadata/Items/Scarabs/ScarabHarbingerGlobal2"},
            {"Harbinger Scarab of Regency", "Metadata/Items/Scarabs/ScarabHarbingerGlobal3"},
            {"Harbinger Scarab of Warhoards", "Metadata/Items/Scarabs/ScarabHarbingerGlobal4"},
            
            // Essence Scarabs
            {"Essence Scarab", "Metadata/Items/Scarabs/ScarabEssenceGlobal1"},
            {"Essence Scarab of Ascent", "Metadata/Items/Scarabs/ScarabEssenceGlobal2"},
            {"Essence Scarab of Stability", "Metadata/Items/Scarabs/ScarabEssenceGlobal3"},
            {"Essence Scarab of Calcification", "Metadata/Items/Scarabs/ScarabEssenceGlobal4"},
            {"Essence Scarab of Adaptation", "Metadata/Items/Scarabs/ScarabEssenceGlobal5"},
            
            // Delirium Scarabs
            {"Delirium Scarab", "Metadata/Items/Scarabs/ScarabDeliriumGlobal1"},
            {"Delirium Scarab of Mania", "Metadata/Items/Scarabs/ScarabDeliriumGlobal2"},
            {"Delirium Scarab of Paranoia", "Metadata/Items/Scarabs/ScarabDeliriumGlobal3"},
            {"Delirium Scarab of Neuroses", "Metadata/Items/Scarabs/ScarabDeliriumGlobal4"},
            {"Delirium Scarab of Delusions", "Metadata/Items/Scarabs/ScarabDeliriumGlobal5"},
            
            // Blight Scarabs
            {"Blight Scarab", "Metadata/Items/Scarabs/ScarabBlightGlobal1"},
            {"Blight Scarab of Bounty", "Metadata/Items/Scarabs/ScarabBlightGlobal2"},
            {"Blight Scarab of the Blightheart", "Metadata/Items/Scarabs/ScarabBlightGlobal3"},
            {"Blight Scarab of Blooming", "Metadata/Items/Scarabs/ScarabBlightGlobal4"},
            {"Blight Scarab of Invigoration", "Metadata/Items/Scarabs/ScarabBlightGlobal5"},
            
            // Ritual Scarabs
            {"Ritual Scarab", "Metadata/Items/Scarabs/ScarabRitualGlobal1"},
            {"Ritual Scarab of Selectiveness", "Metadata/Items/Scarabs/ScarabRitualGlobal2"},
            {"Ritual Scarab of Wisps", "Metadata/Items/Scarabs/ScarabRitualGlobal3"},
            {"Ritual Scarab of Abundance", "Metadata/Items/Scarabs/ScarabRitualGlobal4"},
            
            // Expedition Scarabs
            {"Expedition Scarab", "Metadata/Items/Scarabs/ScarabExpeditionGlobal1"},
            {"Expedition Scarab of Runefinding", "Metadata/Items/Scarabs/ScarabExpeditionGlobal2"},
            {"Expedition Scarab of Verisium Powder", "Metadata/Items/Scarabs/ScarabExpeditionGlobal3"},
            {"Expedition Scarab of the Skald", "Metadata/Items/Scarabs/ScarabExpeditionGlobal4"},
            {"Expedition Scarab of Archaeology", "Metadata/Items/Scarabs/ScarabExpeditionGlobal5"},
            
            // Harvest Scarabs
            {"Harvest Scarab", "Metadata/Items/Scarabs/ScarabHarvestGlobal1"},
            {"Harvest Scarab of Doubling", "Metadata/Items/Scarabs/ScarabHarvestGlobal2"},
            {"Harvest Scarab of Cornucopia", "Metadata/Items/Scarabs/ScarabHarvestGlobal3"},
            
            // Incursion Scarabs
            {"Incursion Scarab", "Metadata/Items/Scarabs/ScarabIncursionGlobal1"},
            {"Incursion Scarab of Invasion", "Metadata/Items/Scarabs/ScarabIncursionGlobal2"},
            {"Incursion Scarab of Champions", "Metadata/Items/Scarabs/ScarabIncursionGlobal3"},
            {"Incursion Scarab of Timelines", "Metadata/Items/Scarabs/ScarabIncursionGlobal4"},
            
            // Betrayal Scarabs
            {"Betrayal Scarab", "Metadata/Items/Scarabs/ScarabBetrayalGlobal1"},
            {"Betrayal Scarab of the Allflame", "Metadata/Items/Scarabs/ScarabBetrayalGlobal2"},
            {"Betrayal Scarab of Reinforcements", "Metadata/Items/Scarabs/ScarabBetrayalGlobal3"},
            {"Betrayal Scarab of Perpetuation", "Metadata/Items/Scarabs/ScarabBetrayalGlobal4"},
            
            // Ambush Scarabs
            {"Ambush Scarab", "Metadata/Items/Scarabs/ScarabAmbushGlobal1"},
            {"Ambush Scarab of Hidden Compartments", "Metadata/Items/Scarabs/ScarabAmbushGlobal2"},
            {"Ambush Scarab of Potency", "Metadata/Items/Scarabs/ScarabAmbushGlobal3"},
            {"Ambush Scarab of Containment", "Metadata/Items/Scarabs/ScarabAmbushGlobal4"},
            {"Ambush Scarab of Discernment", "Metadata/Items/Scarabs/ScarabAmbushGlobal5"},
            
            // Torment Scarabs
            {"Torment Scarab", "Metadata/Items/Scarabs/ScarabTormentGlobal1"},
            {"Torment Scarab of Peculiarity", "Metadata/Items/Scarabs/ScarabTormentGlobal2"},
            {"Torment Scarab of Release", "Metadata/Items/Scarabs/ScarabTormentGlobal3"},
            {"Torment Scarab of Possession", "Metadata/Items/Scarabs/ScarabTormentGlobal4"},
            
            // Beyond Scarabs
            {"Beyond Scarab", "Metadata/Items/Scarabs/ScarabBeyondGlobal1"},
            {"Beyond Scarab of Corruption", "Metadata/Items/Scarabs/ScarabBeyondGlobal2"},
            {"Beyond Scarab of Haemophilia", "Metadata/Items/Scarabs/ScarabBeyondGlobal3"},
            {"Beyond Scarab of Resurgence", "Metadata/Items/Scarabs/ScarabBeyondGlobal4"},
            {"Beyond Scarab of the Invasion", "Metadata/Items/Scarabs/ScarabBeyondGlobal5"},
            
            // Ultimatum Scarabs
            {"Ultimatum Scarab", "Metadata/Items/Scarabs/ScarabUltimatumGlobal1"},
            {"Ultimatum Scarab of Bribing", "Metadata/Items/Scarabs/ScarabUltimatumGlobal2"},
            {"Ultimatum Scarab of Dueling", "Metadata/Items/Scarabs/ScarabUltimatumGlobal3"},
            {"Ultimatum Scarab of Catalysing", "Metadata/Items/Scarabs/ScarabUltimatumGlobal4"},
            {"Ultimatum Scarab of Inscription", "Metadata/Items/Scarabs/ScarabUltimatumGlobal5"},
            
            // Bestiary Scarabs
            {"Bestiary Scarab", "Metadata/Items/Scarabs/ScarabBestiaryGlobal1"},
            {"Bestiary Scarab of the Herd", "Metadata/Items/Scarabs/ScarabBestiaryGlobal2"},
            {"Bestiary Scarab of Duplicating", "Metadata/Items/Scarabs/ScarabBestiaryGlobal3"},
            {"Bestiary Scarab of the Shadowed Crow", "Metadata/Items/Scarabs/ScarabBestiaryGlobal4"},
            
            // Sulphite Scarabs
            {"Sulphite Scarab", "Metadata/Items/Scarabs/ScarabSulphiteGlobal1"},
            {"Sulphite Scarab of Greed", "Metadata/Items/Scarabs/ScarabSulphiteGlobal2"},
            {"Sulphite Scarab of Fumes", "Metadata/Items/Scarabs/ScarabSulphiteGlobal3"},
            
            // Divination Scarabs
            {"Divination Scarab of The Cloister", "Metadata/Items/Scarabs/ScarabDivinationGlobal1"},
            {"Divination Scarab of Plenty", "Metadata/Items/Scarabs/ScarabDivinationGlobal2"},
            {"Divination Scarab of Pilfering", "Metadata/Items/Scarabs/ScarabDivinationGlobal3"},
            
            // Cartography Scarabs
            {"Cartography Scarab of Escalation", "Metadata/Items/Scarabs/ScarabCartographyGlobal1"},
            {"Cartography Scarab of Risk", "Metadata/Items/Scarabs/ScarabCartographyGlobal2"},
            {"Cartography Scarab of Singularity", "Metadata/Items/Scarabs/ScarabCartographyGlobal3"},
            {"Cartography Scarab of Corruption", "Metadata/Items/Scarabs/ScarabCartographyGlobal4"},
            {"Cartography Scarab of the Multitude", "Metadata/Items/Scarabs/ScarabCartographyGlobal5"},
            
            // Influencing Scarabs
            {"Influencing Scarab of the Shaper", "Metadata/Items/Scarabs/ScarabInfluencingGlobal1"},
            {"Influencing Scarab of the Elder", "Metadata/Items/Scarabs/ScarabInfluencingGlobal2"},
            {"Influencing Scarab of Hordes", "Metadata/Items/Scarabs/ScarabInfluencingGlobal3"},
            {"Influencing Scarab of Conversion", "Metadata/Items/Scarabs/ScarabInfluencingGlobal4"},
            
            // Titanic Scarabs
            {"Titanic Scarab", "Metadata/Items/Scarabs/ScarabTitanicGlobal1"},
            {"Titanic Scarab of Treasures", "Metadata/Items/Scarabs/ScarabTitanicGlobal2"},
            {"Titanic Scarab of Legend", "Metadata/Items/Scarabs/ScarabTitanicGlobal3"},
            
            // Kalguuran Scarabs
            {"Kalguuran Scarab", "Metadata/Items/Scarabs/ScarabSettlersGlobal1"},
            {"Kalguuran Scarab of Guarded Riches", "Metadata/Items/Scarabs/ScarabSettlersGlobal2"},
            {"Kalguuran Scarab of Refinement", "Metadata/Items/Scarabs/ScarabSettlersGlobal3"},
            
            // Generic Scarabs
            {"Scarab of Monstrous Lineage", "Metadata/Items/Scarabs/ScarabMapContentGlobal1"},
            {"Scarab of Adversaries", "Metadata/Items/Scarabs/ScarabMapContentGlobal2"},
            {"Scarab of Divinity", "Metadata/Items/Scarabs/ScarabMapContentGlobal3"},
            {"Scarab of Hunted Traitors", "Metadata/Items/Scarabs/ScarabMapContentGlobal4"},
            {"Scarab of Stability", "Metadata/Items/Scarabs/ScarabMapContentGlobal5"},
            {"Scarab of the Commander", "Metadata/Items/Scarabs/ScarabMapContentGlobal6"},
            {"Scarab of Evolution", "Metadata/Items/Scarabs/ScarabMapContentGlobal7"},
            {"Scarab of Wisps", "Metadata/Items/Scarabs/ScarabMapContentGlobal8"},
            {"Scarab of Bisection", "Metadata/Items/Scarabs/ScarabMapContentGlobal9"},
            {"Scarab of Unity", "Metadata/Items/Scarabs/ScarabMapContentGlobal10"},
            {"Scarab of Radiant Storms", "Metadata/Items/Scarabs/ScarabMapContentGlobal11"},
            
            // Horned Scarabs
            {"Horned Scarab of Bloodlines", "Metadata/Items/Scarabs/ScarabHornedGlobal1"},
            {"Horned Scarab of Nemeses", "Metadata/Items/Scarabs/ScarabHornedGlobal2"},
            {"Horned Scarab of Preservation", "Metadata/Items/Scarabs/ScarabHornedGlobal3"},
            {"Horned Scarab of Awakening", "Metadata/Items/Scarabs/ScarabHornedGlobal4"},
            {"Horned Scarab of Tradition", "Metadata/Items/Scarabs/ScarabHornedGlobal5"},
            {"Horned Scarab of Glittering", "Metadata/Items/Scarabs/ScarabHornedGlobal6"},
            {"Horned Scarab of Pandemonium", "Metadata/Items/Scarabs/ScarabHornedGlobal7"},
        };

        public async Task<bool> Run()
        {
            // Log current state for debugging
            GlobalLog.Debug($"[TakeScarabTask] Run() called. UseScarabs={Settings.UseScarabs}, IsHideout={World.CurrentArea.IsHideoutArea}, IsTown={World.CurrentArea.IsTown}, OpenMapTask.Enabled={OpenMapTask.Enabled}");

            // Only run if scarabs are enabled
            if (!Settings.UseScarabs)
            {
                return false;
            }

            // Only run in hideout or town
            if (!World.CurrentArea.IsHideoutArea && !World.CurrentArea.IsTown)
            {
                return false;
            }

            // Only run if we're about to open a map (OpenMapTask is enabled)
            if (!OpenMapTask.Enabled)
            {
                return false;
            }

            // Check which scarabs we need - this already accounts for scarabs in inventory
            var neededScarabs = GetNeededScarabs();
            
            // If no scarabs needed (either none selected or all already in inventory), skip
            if (neededScarabs.Count == 0)
            {
                GlobalLog.Debug("[TakeScarabTask] No scarabs needed (none selected or all already in inventory).");
                return false;
            }

            GlobalLog.Info($"[TakeScarabTask] Need to take {neededScarabs.Count} type(s) of scarabs from stash.");
            
            // Log which scarabs we need
            foreach (var kvp in neededScarabs)
            {
                GlobalLog.Debug($"[TakeScarabTask] Need {kvp.Value}x {kvp.Key}");
            }

            // Open stash
            if (!await Inventories.OpenStash())
            {
                GlobalLog.Error("[TakeScarabTask] Failed to open stash!");
                ErrorManager.ReportError();
                return true;
            }

            GlobalLog.Debug("[TakeScarabTask] Stash opened successfully.");

            // Try to find and take scarabs from fragment stash tab
            var fragmentTabName = GetFragmentTabName();
            if (fragmentTabName == null)
            {
                GlobalLog.Warn("[TakeScarabTask] No fragment stash tab found. Looking in regular tabs...");
                await TakeScarabsFromRegularTabs(neededScarabs);
            }
            else
            {
                GlobalLog.Debug($"[TakeScarabTask] Found fragment tab: {fragmentTabName}");
                
                // Open fragment tab
                if (!await Inventories.OpenStashTab(fragmentTabName))
                {
                    GlobalLog.Error($"[TakeScarabTask] Failed to open fragment tab: {fragmentTabName}");
                    ErrorManager.ReportError();
                    return true;
                }

                // Check if it's actually a Fragment tab type
                var tabType = StashUi.StashTabInfo?.TabType;
                GlobalLog.Debug($"[TakeScarabTask] Tab type: {tabType}");
                
                if (tabType == InventoryTabType.Fragment)
                {
                    await TakeScarabsFromFragmentTab(neededScarabs);
                }
                else
                {
                    GlobalLog.Debug($"[TakeScarabTask] Tab '{fragmentTabName}' is not a Fragment tab (type: {tabType}). Using regular method.");
                    await TakeScarabsFromRegularTab(neededScarabs);
                }
            }

            GlobalLog.Info("[TakeScarabTask] Finished attempting to take scarabs.");
            return true;
        }

        private static Dictionary<string, int> GetNeededScarabs()
        {
            var needed = new Dictionary<string, int>();
            var selectedScarabs = Settings.SelectedScarabs;

            GlobalLog.Debug($"[TakeScarabTask] GetNeededScarabs - Selected scarabs count: {selectedScarabs?.Count ?? 0}");
            
            if (selectedScarabs == null || selectedScarabs.Count == 0)
            {
                GlobalLog.Debug("[TakeScarabTask] No scarabs selected in settings.");
                return needed;
            }

            // Log all selected scarabs
            for (int i = 0; i < selectedScarabs.Count; i++)
            {
                GlobalLog.Debug($"[TakeScarabTask] Selected scarab slot {i}: '{selectedScarabs[i]}'");
            }

            // Count how many of each scarab we need
            foreach (var scarab in selectedScarabs)
            {
                if (string.IsNullOrEmpty(scarab) || scarab == "None") continue;

                if (!needed.ContainsKey(scarab))
                    needed[scarab] = 0;
                needed[scarab]++;
            }

            GlobalLog.Debug($"[TakeScarabTask] Unique scarab types needed: {needed.Count}");

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
                GlobalLog.Debug($"[TakeScarabTask] {kvp.Key}: need {kvp.Value}, have {inInventory} in inventory");
                
                int stillNeeded = kvp.Value - inInventory;

                if (stillNeeded <= 0)
                {
                    needed.Remove(kvp.Key);
                    GlobalLog.Debug($"[TakeScarabTask] Already have enough {kvp.Key} in inventory.");
                }
                else
                {
                    needed[kvp.Key] = stillNeeded;
                    GlobalLog.Debug($"[TakeScarabTask] Still need {stillNeeded}x {kvp.Key}");
                }
            }

            return needed;
        }

        private static string GetFragmentTabName()
        {
            var tabNames = StashUi.TabControl.TabNames;
            
            foreach (var tabName in tabNames)
            {
                var lowerName = tabName.ToLower();
                if (lowerName.Contains("fragment") || lowerName.Contains("scarab") || lowerName.Contains("frag"))
                {
                    return tabName;
                }
            }

            var fragmentTabs = EXtensions.Settings.Instance.GetTabsForCategory(EXtensions.Settings.StashingCategory.Fragment);
            if (fragmentTabs.Count > 0)
            {
                return fragmentTabs[0];
            }

            return null;
        }

        private static async Task TakeScarabsFromFragmentTab(Dictionary<string, int> neededScarabs)
        {
            GlobalLog.Debug("[TakeScarabTask] Using Fragment Tab API to get scarabs...");
            
            foreach (var kvp in neededScarabs.ToList())
            {
                string scarabName = kvp.Key;
                int countNeeded = kvp.Value;

                GlobalLog.Debug($"[TakeScarabTask] Looking for {countNeeded}x {scarabName}");

                // Get the metadata for this scarab
                if (!ScarabMetadata.TryGetValue(scarabName, out string metadata))
                {
                    GlobalLog.Warn($"[TakeScarabTask] Unknown scarab metadata for '{scarabName}'. Trying AllScarab controls...");
                    
                    // Try to find it in AllScarab controls by name
                    bool found = await TryTakeScarabByName(scarabName, countNeeded);
                    if (!found)
                    {
                        GlobalLog.Error($"[TakeScarabTask] Could not find {scarabName} in fragment tab.");
                    }
                    continue;
                }

                GlobalLog.Debug($"[TakeScarabTask] Scarab metadata: {metadata}");

                // Get the inventory control for this scarab using metadata
                var control = FragmentTab.GetInventoryControlForMetadata(metadata);
                if (control == null)
                {
                    GlobalLog.Warn($"[TakeScarabTask] No inventory control found for {scarabName} (metadata: {metadata})");
                    continue;
                }

                // Get the item from the control using CustomTabItem (single slot inventory)
                var item = control.CustomTabItem;
                if (item == null)
                {
                    GlobalLog.Warn($"[TakeScarabTask] No {scarabName} in stash (control exists but empty).");
                    continue;
                }

                GlobalLog.Info($"[TakeScarabTask] Found {item.Name} (Stack: {item.StackCount}) in fragment tab.");

                // Take the scarabs - for single slot inventory, FastMove() doesn't need LocalId
                for (int i = 0; i < countNeeded; i++)
                {
                    // Re-check the item each time since stack may have changed
                    item = control.CustomTabItem;
                    if (item == null || item.StackCount <= 0)
                    {
                        GlobalLog.Warn($"[TakeScarabTask] Ran out of {scarabName} after taking {i}.");
                        break;
                    }

                    GlobalLog.Debug($"[TakeScarabTask] Taking {scarabName} from fragment tab (attempt {i + 1}/{countNeeded})...");
                    
                    // FastMove without LocalId for single slot inventory
                    var result = control.FastMove();
                    if (result != FastMoveResult.None)
                    {
                        GlobalLog.Error($"[TakeScarabTask] FastMove failed for {scarabName}: {result}");
                        break;
                    }

                    GlobalLog.Debug($"[TakeScarabTask] Successfully took {scarabName}.");
                    await Wait.SleepSafe(200);
                }
            }

            GlobalLog.Info("[TakeScarabTask] Finished taking scarabs from fragment tab.");
        }

        private static async Task<bool> TryTakeScarabByName(string scarabName, int countNeeded)
        {
            try
            {
                // Try to find the scarab in AllScarab controls
                var allScarabControls = FragmentTab.AllScarab;
                if (allScarabControls == null)
                {
                    GlobalLog.Error("[TakeScarabTask] FragmentTab.AllScarab is null.");
                    return false;
                }

                foreach (var control in allScarabControls)
                {
                    var item = control.CustomTabItem;
                    if (item != null && item.Name == scarabName)
                    {
                        GlobalLog.Info($"[TakeScarabTask] Found {scarabName} via AllScarab (Stack: {item.StackCount})");
                        
                        for (int i = 0; i < countNeeded; i++)
                        {
                            item = control.CustomTabItem;
                            if (item == null || item.StackCount <= 0) break;

                            var result = control.FastMove();
                            if (result != FastMoveResult.None)
                            {
                                GlobalLog.Error($"[TakeScarabTask] FastMove failed: {result}");
                                break;
                            }
                            await Wait.SleepSafe(200);
                        }
                        return true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                GlobalLog.Error($"[TakeScarabTask] Error in TryTakeScarabByName: {ex.Message}");
            }

            return false;
        }

        private static async Task TakeScarabsFromRegularTabs(Dictionary<string, int> neededScarabs)
        {
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
                    GlobalLog.Info($"[TakeScarabTask] Taking {toTake}x {scarabName} from regular stash.");

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
            GlobalLog.Debug("[TakeScarabTask] Start() called");
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
