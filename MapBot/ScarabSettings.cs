using System.Collections.Generic;
using DreamPoeBot.Loki;
using DreamPoeBot.Loki.Common;
using Newtonsoft.Json;

namespace Default.MapBot
{
    public class ScarabSettings : JsonSettings
    {
        private static ScarabSettings _instance;
        public static ScarabSettings Instance => _instance ?? (_instance = new ScarabSettings());

        private ScarabSettings()
            : base(GetSettingsFilePath(Configuration.Instance.Name, "MapBot", "ScarabSettings.json"))
        {
        }

        // Enable/Disable scarab usage
        public bool UseScarabs { get; set; } = false;

        // Map device slot count (4, 5, or 6)
        public int MapDeviceSlots { get; set; } = 4;

        // Scarab selections for each slot (slot 1 is for map, so we have slots 2-6 for scarabs)
        public string ScarabSlot1 { get; set; } = "None";
        public string ScarabSlot2 { get; set; } = "None";
        public string ScarabSlot3 { get; set; } = "None";
        public string ScarabSlot4 { get; set; } = "None";
        public string ScarabSlot5 { get; set; } = "None";

        // Get all selected scarabs (non-None)
        [JsonIgnore]
        public List<string> SelectedScarabs
        {
            get
            {
                var scarabs = new List<string>();
                int availableSlots = MapDeviceSlots - 1; // -1 for the map slot

                if (availableSlots >= 1 && ScarabSlot1 != "None") scarabs.Add(ScarabSlot1);
                if (availableSlots >= 2 && ScarabSlot2 != "None") scarabs.Add(ScarabSlot2);
                if (availableSlots >= 3 && ScarabSlot3 != "None") scarabs.Add(ScarabSlot3);
                if (availableSlots >= 4 && ScarabSlot4 != "None") scarabs.Add(ScarabSlot4);
                if (availableSlots >= 5 && ScarabSlot5 != "None") scarabs.Add(ScarabSlot5);

                return scarabs;
            }
        }

        // Static list of all available scarabs
        [JsonIgnore]
        public static readonly List<string> AvailableScarabs = new List<string>
        {
            "None",
            // Breach
            "Breach Scarab",
            "Breach Scarab of the Dreamer",
            "Breach Scarab of Lordship",
            "Breach Scarab of Splintering",
            "Breach Scarab of Snares",
            "Breach Scarab of Resonant Cascade",
            // Cartography
            "Cartography Scarab of Escalation",
            "Cartography Scarab of Risk",
            "Cartography Scarab of Singularity",
            "Cartography Scarab of Corruption",
            "Cartography Scarab of the Multitude",
            // Titanic
            "Titanic Scarab",
            "Titanic Scarab of Treasures",
            "Titanic Scarab of Legend",
            // Bestiary
            "Bestiary Scarab",
            "Bestiary Scarab of the Herd",
            "Bestiary Scarab of Duplicating",
            "Bestiary Scarab of the Shadowed Crow",
            // Influencing
            "Influencing Scarab of the Shaper",
            "Influencing Scarab of the Elder",
            "Influencing Scarab of Hordes",
            "Influencing Scarab of Conversion",
            // Sulphite
            "Sulphite Scarab",
            "Sulphite Scarab of Greed",
            "Sulphite Scarab of Fumes",
            // Divination
            "Divination Scarab of The Cloister",
            "Divination Scarab of Plenty",
            "Divination Scarab of Pilfering",
            // Torment
            "Torment Scarab",
            "Torment Scarab of Peculiarity",
            "Torment Scarab of Release",
            "Torment Scarab of Possession",
            // Ambush
            "Ambush Scarab",
            "Ambush Scarab of Hidden Compartments",
            "Ambush Scarab of Potency",
            "Ambush Scarab of Containment",
            "Ambush Scarab of Discernment",
            // Harbinger
            "Harbinger Scarab",
            "Harbinger Scarab of Obelisks",
            "Harbinger Scarab of Regency",
            "Harbinger Scarab of Warhoards",
            // Expedition
            "Expedition Scarab",
            "Expedition Scarab of Runefinding",
            "Expedition Scarab of Verisium Powder",
            "Expedition Scarab of the Skald",
            "Expedition Scarab of Archaeology",
            // Legion
            "Legion Scarab",
            "Legion Scarab of Officers",
            "Legion Scarab of Command",
            "Legion Scarab of The Sekhema",
            "Legion Scarab of Eternal Conflict",
            // Abyss
            "Abyss Scarab",
            "Abyss Scarab of Multitudes",
            "Abyss Scarab of Edifice",
            "Abyss Scarab of Emptiness",
            "Abyss Scarab of Profound Depth",
            // Essence
            "Essence Scarab",
            "Essence Scarab of Ascent",
            "Essence Scarab of Stability",
            "Essence Scarab of Calcification",
            "Essence Scarab of Adaptation",
            // Ritual
            "Ritual Scarab",
            "Ritual Scarab of Selectiveness",
            "Ritual Scarab of Wisps",
            "Ritual Scarab of Abundance",
            // Harvest
            "Harvest Scarab",
            "Harvest Scarab of Doubling",
            "Harvest Scarab of Cornucopia",
            // Incursion
            "Incursion Scarab",
            "Incursion Scarab of Invasion",
            "Incursion Scarab of Champions",
            "Incursion Scarab of Timelines",
            // Betrayal
            "Betrayal Scarab",
            "Betrayal Scarab of the Allflame",
            "Betrayal Scarab of Reinforcements",
            "Betrayal Scarab of Perpetuation",
            // Beyond
            "Beyond Scarab",
            "Beyond Scarab of Corruption",
            "Beyond Scarab of Haemophilia",
            "Beyond Scarab of Resurgence",
            "Beyond Scarab of the Invasion",
            // Ultimatum
            "Ultimatum Scarab",
            "Ultimatum Scarab of Bribing",
            "Ultimatum Scarab of Dueling",
            "Ultimatum Scarab of Catalysing",
            "Ultimatum Scarab of Inscription",
            // Delirium
            "Delirium Scarab",
            "Delirium Scarab of Mania",
            "Delirium Scarab of Paranoia",
            "Delirium Scarab of Neuroses",
            "Delirium Scarab of Delusions",
            // Blight
            "Blight Scarab",
            "Blight Scarab of Bounty",
            "Blight Scarab of the Blightheart",
            "Blight Scarab of Blooming",
            "Blight Scarab of Invigoration",
            // Kalguuran
            "Kalguuran Scarab",
            "Kalguuran Scarab of Guarded Riches",
            "Kalguuran Scarab of Refinement",
            // Generic
            "Scarab of Monstrous Lineage",
            "Scarab of Adversaries",
            "Scarab of Divinity",
            "Scarab of Hunted Traitors",
            "Scarab of Stability",
            "Scarab of the Commander",
            "Scarab of Evolution",
            "Scarab of Wisps",
            "Scarab of Bisection",
            "Scarab of Unity",
            "Scarab of Radiant Storms",
            // Horned
            "Horned Scarab of Bloodlines",
            "Horned Scarab of Nemeses",
            "Horned Scarab of Preservation",
            "Horned Scarab of Awakening",
            "Horned Scarab of Tradition",
            "Horned Scarab of Glittering",
            "Horned Scarab of Pandemonium"
        };

        // Scarab limits dictionary
        [JsonIgnore]
        public static readonly Dictionary<string, int> ScarabLimits = new Dictionary<string, int>
        {
            // Breach
            {"Breach Scarab", 5},
            {"Breach Scarab of the Dreamer", 1},
            {"Breach Scarab of Lordship", 1},
            {"Breach Scarab of Splintering", 2},
            {"Breach Scarab of Snares", 1},
            {"Breach Scarab of Resonant Cascade", 1},
            // Cartography
            {"Cartography Scarab of Escalation", 1},
            {"Cartography Scarab of Risk", 1},
            {"Cartography Scarab of Singularity", 1},
            {"Cartography Scarab of Corruption", 1},
            {"Cartography Scarab of the Multitude", 3},
            // Titanic
            {"Titanic Scarab", 1},
            {"Titanic Scarab of Treasures", 3},
            {"Titanic Scarab of Legend", 1},
            // Bestiary
            {"Bestiary Scarab", 1},
            {"Bestiary Scarab of the Herd", 2},
            {"Bestiary Scarab of Duplicating", 1},
            {"Bestiary Scarab of the Shadowed Crow", 1},
            // Influencing
            {"Influencing Scarab of the Shaper", 1},
            {"Influencing Scarab of the Elder", 1},
            {"Influencing Scarab of Hordes", 1},
            {"Influencing Scarab of Conversion", 1},
            // Sulphite
            {"Sulphite Scarab", 1},
            {"Sulphite Scarab of Greed", 1},
            {"Sulphite Scarab of Fumes", 1},
            // Divination
            {"Divination Scarab of The Cloister", 5},
            {"Divination Scarab of Plenty", 5},
            {"Divination Scarab of Pilfering", 1},
            // Torment
            {"Torment Scarab", 2},
            {"Torment Scarab of Peculiarity", 1},
            {"Torment Scarab of Release", 1},
            {"Torment Scarab of Possession", 3},
            // Ambush
            {"Ambush Scarab", 3},
            {"Ambush Scarab of Hidden Compartments", 1},
            {"Ambush Scarab of Potency", 1},
            {"Ambush Scarab of Containment", 1},
            {"Ambush Scarab of Discernment", 1},
            // Harbinger
            {"Harbinger Scarab", 4},
            {"Harbinger Scarab of Obelisks", 1},
            {"Harbinger Scarab of Regency", 1},
            {"Harbinger Scarab of Warhoards", 1},
            // Expedition
            {"Expedition Scarab", 1},
            {"Expedition Scarab of Runefinding", 2},
            {"Expedition Scarab of Verisium Powder", 1},
            {"Expedition Scarab of the Skald", 1},
            {"Expedition Scarab of Archaeology", 1},
            // Legion
            {"Legion Scarab", 5},
            {"Legion Scarab of Officers", 1},
            {"Legion Scarab of Command", 1},
            {"Legion Scarab of The Sekhema", 1},
            {"Legion Scarab of Eternal Conflict", 1},
            // Abyss
            {"Abyss Scarab", 2},
            {"Abyss Scarab of Multitudes", 2},
            {"Abyss Scarab of Edifice", 1},
            {"Abyss Scarab of Emptiness", 1},
            {"Abyss Scarab of Profound Depth", 1},
            // Essence
            {"Essence Scarab", 2},
            {"Essence Scarab of Ascent", 1},
            {"Essence Scarab of Stability", 1},
            {"Essence Scarab of Calcification", 2},
            {"Essence Scarab of Adaptation", 1},
            // Ritual
            {"Ritual Scarab", 1},
            {"Ritual Scarab of Selectiveness", 2},
            {"Ritual Scarab of Wisps", 1},
            {"Ritual Scarab of Abundance", 2},
            // Harvest
            {"Harvest Scarab", 1},
            {"Harvest Scarab of Doubling", 1},
            {"Harvest Scarab of Cornucopia", 1},
            // Incursion
            {"Incursion Scarab", 1},
            {"Incursion Scarab of Invasion", 3},
            {"Incursion Scarab of Champions", 2},
            {"Incursion Scarab of Timelines", 1},
            // Betrayal
            {"Betrayal Scarab", 1},
            {"Betrayal Scarab of the Allflame", 1},
            {"Betrayal Scarab of Reinforcements", 1},
            {"Betrayal Scarab of Perpetuation", 2},
            // Beyond
            {"Beyond Scarab", 1},
            {"Beyond Scarab of Corruption", 1},
            {"Beyond Scarab of Haemophilia", 2},
            {"Beyond Scarab of Resurgence", 1},
            {"Beyond Scarab of the Invasion", 1},
            // Ultimatum
            {"Ultimatum Scarab", 1},
            {"Ultimatum Scarab of Bribing", 2},
            {"Ultimatum Scarab of Dueling", 1},
            {"Ultimatum Scarab of Catalysing", 1},
            {"Ultimatum Scarab of Inscription", 1},
            // Delirium
            {"Delirium Scarab", 1},
            {"Delirium Scarab of Mania", 2},
            {"Delirium Scarab of Paranoia", 5},
            {"Delirium Scarab of Neuroses", 1},
            {"Delirium Scarab of Delusions", 1},
            // Blight
            {"Blight Scarab", 1},
            {"Blight Scarab of Bounty", 2},
            {"Blight Scarab of the Blightheart", 1},
            {"Blight Scarab of Blooming", 1},
            {"Blight Scarab of Invigoration", 1},
            // Kalguuran
            {"Kalguuran Scarab", 2},
            {"Kalguuran Scarab of Guarded Riches", 1},
            {"Kalguuran Scarab of Refinement", 1},
            // Generic
            {"Scarab of Monstrous Lineage", 2},
            {"Scarab of Adversaries", 2},
            {"Scarab of Divinity", 3},
            {"Scarab of Hunted Traitors", 1},
            {"Scarab of Stability", 1},
            {"Scarab of the Commander", 1},
            {"Scarab of Evolution", 1},
            {"Scarab of Wisps", 2},
            {"Scarab of Bisection", 1},
            {"Scarab of Unity", 1},
            {"Scarab of Radiant Storms", 1},
            // Horned
            {"Horned Scarab of Bloodlines", 1},
            {"Horned Scarab of Nemeses", 2},
            {"Horned Scarab of Preservation", 1},
            {"Horned Scarab of Awakening", 1},
            {"Horned Scarab of Tradition", 1},
            {"Horned Scarab of Glittering", 1},
            {"Horned Scarab of Pandemonium", 1}
        };

        // Get the limit for a scarab (returns 99 if no limit defined - meaning unlimited)
        public static int GetScarabLimit(string scarabName)
        {
            if (ScarabLimits.TryGetValue(scarabName, out int limit))
                return limit;
            return 99; // No limit
        }
    }
}
