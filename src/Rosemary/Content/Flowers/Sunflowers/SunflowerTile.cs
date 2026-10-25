using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Rosemary.Content.Flowers;

public sealed class SunflowerTile : ModTile
{
    // TODO: Move to daybreak hooks
    private sealed class SunflowerGlobal : GlobalItem
    {
        public override bool AppliesToEntity(Item item, bool lateInstantiation) =>
            item.type == ItemID.Sunflower;

        public override void SetDefaults(Item item)
        {
            item.createTile = ModContent.TileType<SunflowerTile>();
        }
    }

    public override string Texture => Assets.Flowers.Sunflowers.SunflowerTile.KEY;

    public override void SetStaticDefaults()
    {
        Main.tileCut[Type] = true;
        Main.tileBlockLight[Type] = false;
        Main.tileLavaDeath[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileFrameImportant[Type] = true;

        TileID.Sets.TileCutIgnore.Regrowth[Type] = true;
        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.ReverseVineThreads[Type] = true;
        TileID.Sets.MultiTileSway[Type] = true;
        TileID.Sets.DoesntGetReplacedWithTileReplacement[Type] = true;

        this.NewTileData(TileObjectData.Style1x1)
            .LavaDeath(true)
            .WaterPlacement(LiquidPlacement.NotAllowed)
            .AnchorValidTiles(
                 TileID.Grass,
                 TileID.GolfGrass,
                 TileID.HallowedGrass,
                 TileID.JungleGrass,
                 TileID.GolfGrassHallowed,
                 TileID.AshGrass
             )
            .Register();

        DustType = DustID.Grass;
        HitSound = SoundID.Grass;

        // AddMapEntry(new Color(200, 170, 108));
    }

    public override bool PreDraw(int i, int j, SpriteBatch sb)
    {
        TileDrawing.Instance.CrawlToBottomOfReverseVineAndAddSpecialPoint(j, i);

        return false;
    }

    public override void PlaceInWorld(int i, int j, Item item)
    {
        var tile = Framing.GetTileSafely(i, j);

        tile.TileFrameY = 18 * 2;
        tile.TileFrameX = (short)(18 * Rand.Next(5));

        NetMessage.SendTileSquare(-1, i, j);
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        tileFrameX = (short)((tileFrameX / 18) * 34);
        width = 32;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
    {
        if (i % 2 == 0)
        {
            spriteEffects = SpriteEffects.FlipHorizontally;
        }
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        var above = Framing.GetTileSafely(i, j - 1);
        var tile = Framing.GetTileSafely(i, j);
        var below = Framing.GetTileSafely(i, j + 1);

        if (!above.HasTile || above.TileType != tile.TileType)
        {
            tile.TileFrameY = below.TileType == tile.TileType ? (short)(0) : (short)(18 * 2);
            tile.TileFrameX = (short)(18 * Rand.Next(5));
        }

        return !below.HasTile;
    }

    public override void RandomUpdate(int i, int j, bool underground)
    {
        if (underground)
        {
            return;
        }

        var tile = Framing.GetTileSafely(i, j);
        var above = Framing.GetTileSafely(i, j - 1);
        var below = Framing.GetTileSafely(i, j + 1);

        if (above.HasTile || above.LiquidAmount > 0)
        {
            return;
        }

        if (below.TileType == tile.TileType)
        {
            tile.TileFrameY = 18;
            tile.TileFrameX = (short)(18 * Rand.Next(5));
        }

        above.TileType = tile.TileType;
        above.TileFrameY = 0;
        above.TileFrameX = (short)(18 * Rand.Next(5));

        above.HasTile = true;
        above.CopyPaintAndCoating(tile);

        WorldGen.SquareTileFrame(i, j);

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendTileSquare(-1, i, j);
        }
    }
}
