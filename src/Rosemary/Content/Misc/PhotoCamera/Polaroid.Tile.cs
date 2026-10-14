using Microsoft.Xna.Framework;
using Rosemary.Common;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

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
            .UsesCustomCanPlace(true)
            .Size(1, 1)
            .Origin(0, 0)
            .FlattenAnchors(true)
            .DrawStepDown(4)
            .CoordinateWidth(20)
            .CoordinateHeights(22)
            .CoordinatePadding(2)
            .StyleMultiplier(3)
            .WaterPlacement(LiquidPlacement.NotAllowed)
            .LavaPlacement(LiquidPlacement.NotAllowed)
            .WaterDeath(false)
            .LavaDeath(true)
                .NewAlternateTileFromParent(1)
                .AnchorLeft(new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, 1, 0))
                .Register()
                .NewAlternateTileFromParent(2)
                .AnchorRight(new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, 1, 0))
                .Register()
                .NewAlternateTileFromParent(3)
                .RandomStyleRange(2)
                .AnchorWall(true)
                .Register()
            .AnchorBottom(new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 1, 0))
            .Register();

        AddMapEntry(new Color(219, 237, 228), Mods.Rosemary.Content.Misc.PolaroidTile.MapName.GetText());
    }
}