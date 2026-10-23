using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.GameContent.Drawing;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Content.Misc;

public sealed class ConspiratorialWallItem : ModItem
{
    public override string Texture => Assets.Misc.Furniture.ConspiratorialWallItem.KEY;

    public override string LocalizationCategory => "Content.Misc";

    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 400;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableWall(ModContent.WallType<ConspiratorialWall>());
    }

    public override void AddRecipes()
    {
        // TODO
    }
}

public sealed class ConspiratorialWall : ModWall
{
    public override string Texture => Assets.Misc.Furniture.ConspiratorialWall.KEY;

    public override void SetStaticDefaults()
    {
        Main.wallHouse[Type] = true;

        DustType = DustID.WoodFurniture;

        AddMapEntry(new Color(90, 58, 38));
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    private static readonly Point[] offsets =
    [
        new(-1, -1), new(18, 0), new(18, 36), new(54, 0),
        new(0, 18), new(0, 0), new(0, 36), new(90, 0),
        new(36, 18), new(36, 0), new(36, 36), new(90, 18),
        new(54, 18), new(72, 0), new(72, 18), new(18, 18),
    ];

    public override void PostDraw(int i, int j, SpriteBatch sb)
    {
        var center = Framing.GetTileSafely(i, j);

        var texture = WallDrawing.Instance.GetWallDrawTexture(center);

        var mask = GetMask();

        if (mask == 0)
        {
            return;
        }

        var frameOffset = offsets[mask];

        var frame = new Rectangle(frameOffset.X, 180 + frameOffset.Y, 16, 16);

        var off = new Vector2(Main.offScreenRange);

        var position = new Point(i, j).ToWorldCoordinates(0f, 0f) - Main.screenPosition + off;

        Lighting.GetCornerColors(i, j, out var vertices);

        if (center.IsWallFullbright)
        {
            vertices = new VertexColors(Color.White);
        }

        Main.tileBatch.Draw(texture, position, frame, vertices);

        return;

        int GetMask()
        {
            var mask = 0;

            Check(Framing.GetTileSafely(i, j - 1), 1);
            Check(Framing.GetTileSafely(i, j + 1), 2);
            Check(Framing.GetTileSafely(i - 1, j), 4);
            Check(Framing.GetTileSafely(i + 1, j), 8);

            return mask;

            void Check(Tile tile, int bit)
            {
                if (!tile.HasWall
                 || tile.IsWallInvisible
                 || tile.WallType == Type)
                {
                    return;
                }

                mask |= bit;
            }
        }
    }
}
