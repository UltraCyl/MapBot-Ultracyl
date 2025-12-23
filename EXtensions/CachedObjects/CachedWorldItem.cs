using Default.EXtensions.Positions;
using DreamPoeBot.Common;
using DreamPoeBot.Loki.Common;
using DreamPoeBot.Loki.Game.GameData;
using DreamPoeBot.Loki.Game.Objects;

namespace Default.EXtensions.CachedObjects
{
    public class CachedWorldItem : CachedObject
    {
        public Vector2i Size { get; }
        public Rarity Rarity { get; }

        public CachedWorldItem(int id, WalkablePosition position, Vector2i size, Rarity rarity)
            : base(id, position)
        {
            Size = size;
            Rarity = rarity;
        }

        public new WorldItem Object => GetObject() as WorldItem;
    }
}