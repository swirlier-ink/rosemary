using Rosemary.Common;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Content.Flowers;

public static class RoseThornsWorldGen
{
    [GlobalTileHooks.RandomUpdate]
    private static void RandomUpdate(int i, int j, int type, bool underground)
    {
        if (!((!underground && Main.IsItDay()) || Main.remixWorld)
         || !Main.hardMode
         || type != TileID.Grass
         || !Rand.NextBool(520))
        {
            return;
        }

        var tile = Framing.GetTileSafely(i, j - 1);
        var below = Framing.GetTileSafely(i, j);

        if (!below.HasTile || !below.Slope.Block || below.IsHalfBlock)
        {
            return;
        }

        if (tile.HasTile
         && tile.TileType != TileID.Plants)
        {
            return;
        }

        WorldGen.PlaceTile(i, j - 1, ModContent.TileType<RoseThorn>(), true);
        tile.CopyPaintAndCoating(below);

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendTileSquare(-1, i, j);
        }
    }
}
