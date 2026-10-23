using Terraria;
// ReSharper disable InconsistentNaming

namespace Rosemary.Common;

public static partial class WorldItemExtensions
{
    extension(WorldItem item)
    {
        public int alpha => item.inner.alpha;

        public int ammo => item.inner.ammo;

        public int buffType => item.inner.buffType;

        public int createTile => item.inner.createTile;

        public int damage => item.inner.damage;

        public bool expert => item.inner.expert;

        public bool favorited
        {
            get => item.inner.favorited;
            set => item.inner.favorited = value;
        }

        public int glowMask => item.inner.glowMask;

        public float knockBack => item.inner.knockBack;

        public int makeNPC
        {
            get => item.inner.makeNPC;
            set => item.inner.makeNPC = value;
        }

        public bool newAndShiny
        {
            get => item.inner.newAndShiny;
            set => item.inner.newAndShiny = value;
        }

        public bool notAmmo => item.inner.notAmmo;

        public int placeStyle => item.inner.placeStyle;

        public int rare => item.inner.rare;

        public int shoot => item.inner.shoot;

        public float shootSpeed => item.inner.shootSpeed;

        public int useAmmo => item.inner.useAmmo;

        public int useAnimation => item.inner.useAnimation;

        public int useTime => item.inner.useTime;

        public void SetDefaults(int type)
        {
            item.ResetStats(type);
            item.inner.SetDefaults(type);
        }

        public void ResetStats(int Type)
        {
            item.inner.ResetStats(Type);
            item.wet = false;
            item.wetCount = 0;
            item.lavaWet = false;
            item.timeSinceTheItemHasBeenReservedForSomeone = 0;
            item.instanced = false;
        }
    }
}
