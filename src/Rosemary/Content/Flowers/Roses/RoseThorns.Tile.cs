using Rosemary.Common;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Content.Flowers;

public sealed class RoseThorn : ModTile
{
    public override string Texture => Assets.Flowers.Roses.RoseThornTile.KEY;

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileCut[Type] = true;
        Main.tileNoFail[Type] = true;

        TileID.Sets.TileCutIgnore.IgnoreDontHurtNature[Type] = true;
        TileID.Sets.GetsDestroyedForMeteors[Type] = true;

        TileID.Sets.TouchDamageDestroyTile[Type] = true;
        TileID.Sets.TouchDamageImmediate[Type] = 25;

        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;

        TileID.Sets.SpawnsNatureFlies[Type] = true;

        TileID.Sets.ChecksForMerge[Type] = true;
        Main.tileMerge[Type][TileID.Grass] = true;

        DustType = DustID.Grass;
        HitSound = SoundID.Grass;

        // AddMapEntry(new Color(54, 154, 54), Lang.GetItemName(ItemID.Sunflower));

        RegisterItemDrop(ItemID.None);
    }

    public override void RandomUpdate(int i, int j, bool underground)
    {
        if (underground
         || !Rand.NextBool(5))
        {
            return;
        }

        WorldGen.GrowSpike(i, j, Type, TileID.Grass);
    }
}
