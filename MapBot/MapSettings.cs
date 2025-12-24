using System.Collections.Generic;
using System.IO;
using System.Linq;
using Default.EXtensions;
using DreamPoeBot.Loki;
using Newtonsoft.Json;

namespace Default.MapBot
{
    public class MapSettings
    {
        private static readonly string SettingsPath = Path.Combine(Configuration.Instance.Path, "MapBot", "MapSettings.json");

        private static MapSettings _instance;
        public static MapSettings Instance => _instance ?? (_instance = new MapSettings());

        private MapSettings()
        {
            InitList();
            Load();
            InitDict();
            Configuration.OnSaveAll += (sender, args) => { Save(); };

            MapList = MapList.OrderByDescending(m => m.Priority).ToList();
        }

        public List<MapData> MapList { get; } = new List<MapData>();
        public Dictionary<string, MapData> MapDict { get; } = new Dictionary<string, MapData>();

        private void InitList()
        {
            // Tier 1 Maps
            MapList.Add(new MapData(MapNames.BoneCrypt, 1, MapType.Regular));
            MapList.Add(new MapData(MapNames.Shipyard, 1, MapType.Regular));
            MapList.Add(new MapData(MapNames.Thicket, 1, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Academy, 1, MapType.Bossroom));

            // Tier 2 Maps
            MapList.Add(new MapData(MapNames.Dunes, 2, MapType.Regular));
            MapList.Add(new MapData(MapNames.Volcano, 2, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.ToxicSewer, 2, MapType.Regular));
            MapList.Add(new MapData(MapNames.Waterways, 2, MapType.Regular));
            MapList.Add(new MapData(MapNames.FloodedMine, 2, MapType.Regular));
            MapList.Add(new MapData(MapNames.TropicalIsland, 2, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.Core, 2, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Residence, 2, MapType.Multilevel));

            // Tier 3 Maps
            MapList.Add(new MapData(MapNames.OvergrownShrine, 3, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Strand, 3, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Excavation, 3, MapType.Complex));
            MapList.Add(new MapData(MapNames.Palace, 3, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Coves, 3, MapType.Regular));
            MapList.Add(new MapData(MapNames.CastleRuins, 3, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Summit, 3, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Canyon, 3, MapType.Regular));

            // Tier 4 Maps
            MapList.Add(new MapData(MapNames.Cemetery, 4, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.MoonTemple, 4, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.VaalPyramid, 4, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.JungleValley, 4, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Mesa, 4, MapType.Regular));
            MapList.Add(new MapData(MapNames.Precinct, 4, MapType.Regular));
            MapList.Add(new MapData(MapNames.Factory, 4, MapType.Regular));
            MapList.Add(new MapData(MapNames.Silo, 4, MapType.Regular));

            // Tier 5 Maps
            MapList.Add(new MapData(MapNames.Atoll, 5, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Courtyard, 5, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.LavaChamber, 5, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Colosseum, 5, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.BrambleValley, 5, MapType.Regular));
            MapList.Add(new MapData(MapNames.Iceberg, 5, MapType.Regular));
            MapList.Add(new MapData(MapNames.Ghetto, 5, MapType.Regular));
            MapList.Add(new MapData(MapNames.CrimsonTemple, 5, MapType.Bossroom));

            // Tier 6 Maps
            MapList.Add(new MapData(MapNames.Shore, 6, MapType.Regular));
            MapList.Add(new MapData(MapNames.Promenade, 6, MapType.Regular));
            MapList.Add(new MapData(MapNames.Arsenal, 6, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Lookout, 6, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.SpiderForest, 6, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.WastePool, 6, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Sepulchre, 6, MapType.Bossroom));

            // Tier 7 Maps
            MapList.Add(new MapData(MapNames.CursedCrypt, 7, MapType.Regular));
            MapList.Add(new MapData(MapNames.Tower, 7, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.InfestedValley, 7, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.GraveTrough, 7, MapType.Regular));
            MapList.Add(new MapData(MapNames.CitySquare, 7, MapType.Regular));
            MapList.Add(new MapData(MapNames.PrimordialBlocks, 7, MapType.Regular));
            MapList.Add(new MapData(MapNames.Estuary, 7, MapType.Bossroom));

            // Tier 8 Maps
            MapList.Add(new MapData(MapNames.Alleyways, 8, MapType.Regular));
            MapList.Add(new MapData(MapNames.Grotto, 8, MapType.Regular));
            MapList.Add(new MapData(MapNames.Channel, 8, MapType.Regular));
            MapList.Add(new MapData(MapNames.Port, 8, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.ArachnidNest, 8, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Pit, 8, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Bazaar, 8, MapType.Bossroom));

            // Tier 9 Maps
            MapList.Add(new MapData(MapNames.Mausoleum, 9, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.DarkForest, 9, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.AridLake, 9, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Glacier, 9, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Villa, 9, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.LavaLake, 9, MapType.Bossroom));

            // Tier 10 Maps
            MapList.Add(new MapData(MapNames.UndergroundSea, 10, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.PrimordialPool, 10, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.MineralPools, 10, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Cells, 10, MapType.Regular));
            MapList.Add(new MapData(MapNames.FrozenCabins, 10, MapType.Regular));
            MapList.Add(new MapData(MapNames.Crater, 10, MapType.Bossroom));

            // Tier 11 Maps
            MapList.Add(new MapData(MapNames.Arcade, 11, MapType.Regular));
            MapList.Add(new MapData(MapNames.AshenWood, 11, MapType.Regular));
            MapList.Add(new MapData(MapNames.Belfry, 11, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Ramparts, 11, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.AcidCaverns, 11, MapType.Regular));
            MapList.Add(new MapData(MapNames.Desert, 11, MapType.Regular));

            // Tier 12 Maps
            MapList.Add(new MapData(MapNames.Museum, 12, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Necropolis, 12, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.MudGeyser, 12, MapType.Regular));
            MapList.Add(new MapData(MapNames.Park, 12, MapType.Regular));
            MapList.Add(new MapData(MapNames.CrimsonTownship, 12, MapType.Regular));
            MapList.Add(new MapData(MapNames.Marshes, 12, MapType.Bossroom));

            // Tier 13 Maps
            MapList.Add(new MapData(MapNames.DefiledCathedral, 13, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Lighthouse, 13, MapType.Regular));
            MapList.Add(new MapData(MapNames.DesertSpring, 13, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.BurialChambers, 13, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.Chateau, 13, MapType.Regular));

            // Tier 14 Maps
            MapList.Add(new MapData(MapNames.Temple, 14, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Maze, 14, MapType.Regular));
            MapList.Add(new MapData(MapNames.Racecourse, 14, MapType.Multilevel));
            MapList.Add(new MapData(MapNames.Orchard, 14, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.ArachnidTomb, 14, MapType.Multilevel));

            // Tier 15 Maps
            MapList.Add(new MapData(MapNames.UndergroundRiver, 15, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.SulphurVents, 15, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Plateau, 15, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Arena, 15, MapType.Complex));
            MapList.Add(new MapData(MapNames.Phantasmagoria, 15, MapType.Bossroom));

            // Tier 16 Maps
            MapList.Add(new MapData(MapNames.IvoryTemple, 16, MapType.Complex));
            MapList.Add(new MapData(MapNames.Beach, 16, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.ForkingRiver, 16, MapType.Regular));
            MapList.Add(new MapData(MapNames.Wasteland, 16, MapType.Regular));

            // Tier 17 Maps (Uber Endgame)
            MapList.Add(new MapData(MapNames.Sanctuary, 17, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Citadel, 17, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Fortress, 17, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Abomination, 17, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.Ziggurat, 17, MapType.Bossroom));

            // Guardian Maps (Tier 16+)
            MapList.Add(new MapData(MapNames.VaalTemple, 16, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.ForgeOfPhoenix, 16, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.LairOfHydra, 16, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.MazeOfMinotaur, 16, MapType.Bossroom));
            MapList.Add(new MapData(MapNames.PitOfChimera, 16, MapType.Bossroom));

            // Unique Maps (Ignored by default)
            MapList.Add(new MapData(MapNames.VaultsOfAtziri, 3, MapType.Regular) {Ignored = true});
            MapList.Add(new MapData(MapNames.WhakawairuaTuahu, 6, MapType.Multilevel) {Ignored = true});
            MapList.Add(new MapData(MapNames.OlmecSanctum, 7, MapType.Complex) {Ignored = true});
            MapList.Add(new MapData(MapNames.MaelstromOfChaos, 8, MapType.Bossroom) {Ignored = true});
            MapList.Add(new MapData(MapNames.MaoKun, 9, MapType.Regular) {Ignored = true});
            MapList.Add(new MapData(MapNames.PoorjoyAsylum, 9, MapType.Regular) {Ignored = true});
            MapList.Add(new MapData(MapNames.PutridCloister, 9, MapType.Multilevel) {Ignored = true});
            MapList.Add(new MapData(MapNames.CaerBlaiddWolfpackDen, 10, MapType.Bossroom) {Ignored = true});
            MapList.Add(new MapData(MapNames.Beachhead, 15, MapType.Bossroom) {Ignored = true});
        }

        private void InitDict()
        {
            foreach (var data in MapList)
            {
                if (!MapDict.ContainsKey(data.Name))
                {
                    MapDict.Add(data.Name, data);
                }
            }
        }

        private void Load()
        {
            if (!File.Exists(SettingsPath))
                return;

            var json = File.ReadAllText(SettingsPath);
            if (string.IsNullOrWhiteSpace(json))
            {
                GlobalLog.Error("[MapBot] Fail to load \"MapSettings.json\". File is empty.");
                return;
            }
            var parts = JsonConvert.DeserializeObject<Dictionary<string, EditablePart>>(json);
            if (parts == null)
            {
                GlobalLog.Error("[MapBot] Fail to load \"MapSettings.json\". Json deserealizer returned null.");
                return;
            }
            foreach (var data in MapList)
            {
                if (parts.TryGetValue(data.Name, out var part))
                {
                    data.Priority = part.Priority;
                    data.Ignored = part.Ignore;
                    data.IgnoredBossroom = part.IgnoreBossroom;
                    data.Sextant = part.Sextant;
                    data.ZanaMod = part.ZanaMod;
                    data.MobRemaining = part.MobRemaining;
                    data.StrictMobRemaining = part.StrictMobRemaining;
                    data.ExplorationPercent = part.ExplorationPercent;
                    data.StrictExplorationPercent = part.StrictExplorationPercent;
                    data.TrackMob = part.TrackMob;
                    data.FastTransition = part.FastTransition;
                }
            }
        }

        private void Save()
        {
            var parts = new Dictionary<string, EditablePart>(MapList.Count);

            foreach (var map in MapList)
            {
                var part = new EditablePart
                {
                    Priority = map.Priority,
                    Ignore = map.Ignored,
                    IgnoreBossroom = map.IgnoredBossroom,
                    Sextant = map.Sextant,
                    ZanaMod = map.ZanaMod,
                    MobRemaining = map.MobRemaining,
                    StrictMobRemaining = map.StrictMobRemaining,
                    ExplorationPercent = map.ExplorationPercent,
                    StrictExplorationPercent = map.StrictExplorationPercent,
                    TrackMob = map.TrackMob,
                    FastTransition = map.FastTransition
                };
                parts.Add(map.Name, part);
            }
            var json = JsonConvert.SerializeObject(parts, Formatting.Indented);
            File.WriteAllText(SettingsPath, json);
        }

        private class EditablePart
        {
            public int Priority;
            public bool Ignore;
            public bool IgnoreBossroom;
            public bool Sextant;
            public int ZanaMod;
            public int MobRemaining;
            public bool StrictMobRemaining;
            public int ExplorationPercent;
            public bool StrictExplorationPercent;
            public bool? TrackMob;
            public bool? FastTransition;
        }
    }
}
