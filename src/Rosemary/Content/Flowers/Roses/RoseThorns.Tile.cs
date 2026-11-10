using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Rosemary.Content.Flowers;

public sealed class RoseThorn : ModTile
{
    public override string Texture => Assets.Flowers.Roses.RoseThornTile.KEY;

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileCut[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileBlockLight[Type] = true;

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

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        height += 2;
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

    public override bool PreDraw(int i, int j, SpriteBatch sb)
    {
        TileDrawing.Instance.AddSpecialPoint(i, j, TileDrawing.TileCounterType.CustomNonSolid);

        return true;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override void SpecialDraw(int i, int j, SpriteBatch sb)
    {
        var tile = Framing.GetTileSafely(i, j);

        var rand = new FastRandom(i * j);

        if (rand.Next(5) != 0)
        {
            return;
        }

        var texture = TileDrawing.Instance.GetTileDrawTexture(tile, i, j);

        var position = new Point(i, j).ToWorldCoordinates(0f, 0f);
        position -= Main.screenPosition;
        position += (new Vector2(rand.NextFloat(), rand.NextFloat()) * 8).Round() * 2;

        var frame = new Rectangle(36 * rand.Next(8), 90, 36, 36);

        var origin = frame.Size() * 0.5f;

        var color = Lighting.GetColor(i, j);

        if (tile.IsTileFullbright)
        {
            color = Color.White;
        }

        var effects = SpriteEffects.None;

        if (rand.Next(2) == 0)
        {
            effects |= SpriteEffects.FlipHorizontally;
        }
        if (rand.Next(2) == 0)
        {
            effects |= SpriteEffects.FlipVertically;
        }

        sb.Draw(texture, position, frame, color, 0f, origin, 1f, effects, 0f);
    }
}
