using Microsoft.Xna.Framework;
using Rosemary.Common;
using System;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace Rosemary.Content.Misc;

// TODO: Config?
partial class MiscProjectileSets
{
    [OnLoad]
    private static void Load_AffectsWindGrid()
    {
        On_WindGrid.Update += Update_AffectsWindGrid;
    }

    private static void Update_AffectsWindGrid(On_WindGrid.orig_Update orig, WindGrid self)
    {
        orig(self);

        var wind = TileDrawing.Instance.Wind;

        var screenSize = new Vector2(Main.screenWidth, Main.screenHeight);

        var padding = new Vector2(16f);

        foreach (var p in Main.ActiveProjectiles)
        {
            if (!ProjectileID.Sets.AffectsWindGrid[p.type])
            {
                continue;
            }

            if (p.type == ProjectileID.LastPrismLaser)
            {
                LastPrism(p);
                continue;
            }

            if (!Collision.CheckAABBvAABBCollision(Main.screenPosition, screenSize, p.position - padding, p.Size + padding))
            {
                continue;
            }

            var directionX = MathF.Sign(p.velocity.X);
            var directionY = MathF.Sign(p.velocity.Y);

            foreach (var (i, j) in Collision.GetTilesIn(p.TopLeft, p.BottomRight))
            {
                wind.SetWindTime(i, j, directionX, directionY);
            }
        }

        return;

        void LastPrism(Projectile p)
        {
            var directionX = MathF.Sign(p.velocity.X);
            var directionY = MathF.Sign(p.velocity.Y);

            for (var k = 0f; k < p.localAI[1]; k += 16f)
            {
                var pos = p.Center + p.velocity * k;

                var (i, j) = pos.ToTileCoordinates();

                var indexX = i % wind._width;
                var indexY = j % wind._height;

                if (wind._gameTime - wind._grid[indexX, indexY].Time > 25 && Rand.NextBool(4))
                {
                    wind.SetWindTime(i, j, directionX, directionY);
                }
            }
        }
    }
}
