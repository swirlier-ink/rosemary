using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Rosemary.Content.Flowers;

public sealed class RoseDust : ModDust
{
    public override string Texture => Assets.Flowers.Roses.RoseDust.KEY;

    public override void OnSpawn(Dust dust)
    {
        dust.frame = new Rectangle(0, Main.rand.Next(3) * 12, 12, 12);
    }

    public override bool Update(Dust dust)
    {
        dust.velocity.X += Main.WindForVisuals * 0.043f;
        dust.velocity.Y += 0.065f;

        dust.position += dust.velocity;

        dust.rotation += dust.velocity.X * 0.1f;

        dust.scale *= 0.98f;

        if (dust.scale < 0.05f)
        {
            dust.active = false;
        }

        return false;
    }
}

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

        // TileID.Sets.SpawnsNatureFlies[Type] = true;

        TileID.Sets.ChecksForMerge[Type] = true;
        Main.tileMerge[Type][TileID.Grass] = true;

        DustType = DustID.GrassBlades;
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

    public override bool CreateDust(int i, int j, ref int type)
    {
        var rand = new FastRandom(i * j);

        if (rand.Next(5) != 0)
        {
            return true;
        }

        var position = new Point(i, j).ToWorldCoordinates();

        var offset = Rand.NextSquare(-10f, 10f);

        var velocity = Vector2.Normalize(offset) * Rand.Next(0.3f, 2.1f);
        velocity -= Vector2.UnitY * 0.6f;

        Dust.NewDustPerfect(position + offset, ModContent.DustType<RoseDust>(), velocity);

        return true;
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

        var tileLight = Lighting.GetColor(i, j);
        TileDrawing.Instance.DrawAnimatedTile_AdjustForVisionChangers(i, j, tile, tile.TileType, tile.TileFrameX, tile.TileFrameY, ref tileLight, Rand.NextBool(4));
        tileLight = TileDrawing.Instance.DrawTiles_GetLightOverride(j, i, tile, tile.TileType, tile.TileFrameX, tile.TileFrameY, tileLight);

        var effects = SpriteEffects.None;

        if (rand.Next(2) == 0)
        {
            effects |= SpriteEffects.FlipHorizontally;
        }
        if (rand.Next(2) == 0)
        {
            effects |= SpriteEffects.FlipVertically;
        }

        var rotation = TileDrawing.Instance.GetWindCycle(i, j, TileDrawing.Instance._grassWindCounter);

        if (!WallID.Sets.AllowsWind[tile.WallType])
        {
            rotation = 0f;
        }
        if (!TileDrawing.Instance.InAPlaceWithWind(i, j, 1, 1))
        {
            rotation = 0f;
        }

        TileDrawing.Instance.GetWindGridPush2Axis(i, j, 20, 0.35f, out var pushX, out var pushY);

        rotation += pushX + pushY;

        rotation *= 0.1f;

        sb.Draw(texture, position, frame, tileLight, rotation, origin, 1f, effects, 0f);

        TileDrawing.EmitNatureFlies(i, j, 450);
    }
}
