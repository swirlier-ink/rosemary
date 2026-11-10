using Microsoft.Xna.Framework;
using System;
using Rosemary.Common;
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

    // TODO: Last Prism edge case
    private static void Update_AffectsWindGrid(On_WindGrid.orig_Update orig, WindGrid self)
    {
        orig(self);

        var screenSize = new Vector2(Main.screenWidth, Main.screenHeight);

        var padding = new Vector2(16f);

        foreach (var p in Main.ActiveProjectiles)
        {
            if (!ProjectileID.Sets.AffectsWindGrid[p.type]
             || !Collision.CheckAABBvAABBCollision(Main.screenPosition, screenSize, p.position - padding, p.Size + padding))
            {
                continue;
            }

            var directionX = MathF.Sign(p.velocity.X);
            var directionY = MathF.Sign(p.velocity.Y);

            foreach (var (i, j) in Collision.GetTilesIn(p.TopLeft, p.BottomRight))
            {
                TileDrawing.Instance.Wind.SetWindTime(i, j, directionX, directionY);
            }
        }
    }
}
