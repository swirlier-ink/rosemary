using Microsoft.Xna.Framework;
using Rosemary.Common;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Content.Misc;

public sealed class PolaroidTile : ModTile
{
    public override string Texture => Assets.Misc.Camera.Polaroid_Tile.KEY;

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;

        Main.tileWaterDeath[Type] = false;
        Main.tileLavaDeath[Type] = true;

        Main.tileSolid[Type] = false;
        Main.tileNoAttach[Type] = true;
        Main.tileNoFail[Type] = true;

        TileID.Sets.FramesOnKillWall[Type] = true;

        this.NewTileData()
            .Size(1, 1)
            .Origin(0, 0)
            .FlattenAnchors(true)
            .CoordinateWidth(16)
            .CoordinateHeights(22)
            .CoordinatePadding(2)
            .StyleWrapLimit(8)
            .RandomStyleRange(2)
            .StyleHorizontal(true)
            .WaterPlacement(LiquidPlacement.NotAllowed)
            .LavaPlacement(LiquidPlacement.NotAllowed)
            .WaterDeath(false)
            .LavaDeath(true)
            .AddAlternateTile(
                 2,
                 b => b.DrawYOffset(-2)
                       .AnchorWall(true)
                       .Calculate()
             )
            .AddAlternateTile(
                 4,
                 b => b.AnchorLeft(new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, 1, 0))
                       .Calculate()
             )
            .AddAlternateTile(
                 6,
                 b => b.AnchorRight(new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, 1, 0))
                       .Calculate()
             )
            .AnchorBottom(new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 1, 0))
            .DrawYOffset(-4)
            .Register();

        AddMapEntry(new Color(219, 237, 228), Mods.Rosemary.Content.Misc.PolaroidTile.MapName.GetText());
    }
}