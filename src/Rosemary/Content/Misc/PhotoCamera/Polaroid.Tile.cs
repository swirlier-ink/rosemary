using Microsoft.Xna.Framework;
using Rosemary.Common;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Rosemary.Content.Misc;

public sealed class PolaroidTile : ModTile
{
    public override string Texture => Assets.Misc.Camera.Polaroid_Tile.KEY;

    public override void Load()
    {
        On_Main.ClearHoverItem += ClearHoverItem_StupidTooltipFix;
        IL_Main.DoDraw += _ => { };
    }

    private static Item? hoverItem;

    private static void ClearHoverItem_StupidTooltipFix(On_Main.orig_ClearHoverItem orig)
    {
        orig();

        if (hoverItem is null)
        {
            return;
        }

        var item = hoverItem;

        if (item.ModItem is PolaroidItem polaroid)
        {
            polaroid.ShowTooltip = true;
        }

        Main.HoverItem = item.Clone();
        Main.instance.MouseText(item.Name, item.rare);

        hoverItem = null;
    }

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
            .HookPostPlaceMyPlayer(ModContent.GetInstance<PolaroidTileEntity>().PlacePolaroidHook)
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

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = 0;
    }

    public override void MouseOver(int i, int j)
    {
        if (TileEntity.TryGet(i, j, out PolaroidTileEntity tileEntity))
        {
            hoverItem = tileEntity.Polaroid;
        }

        var player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = false;
        player.cursorItemIconID = -1;
    }

    public override bool RightClick(int i, int j)
    {
        WorldGen.KillTile(i, j);

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j);
        }

        Main.LocalPlayer.GamepadEnableGrappleCooldown();

        return true;
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        ModContent.GetInstance<PolaroidTileEntity>().Kill(i, j);
    }
}

public sealed class PolaroidTileEntity : ModTileEntity
{
    public PlacementHook PlacePolaroidHook => new(HookPostPlaceMyPlayer_PlacePolaroid, -1, 0, processedCoordinates: true);

    public Item Polaroid { get; set; } = new();

    private int HookPostPlaceMyPlayer_PlacePolaroid(int i, int j, int type, int style, int direction, int alternate)
    {
        var id = Generic_Hook_AfterPlacement(i, j, type, style, direction, alternate);

        var tileEntity = (PolaroidTileEntity)ByID[id];

        var player = Main.LocalPlayer;

        // Should be reliable?
        tileEntity.Polaroid = player.inventory[player.selectedItem].Clone();
        tileEntity.Polaroid.stack = 1;

        return id;
    }

    public override bool IsTileValidForEntity(int x, int y)
    {
        var tile = Main.tile[x, y];
        return tile.HasTile
            && tile.TileType == ModContent.TileType<PolaroidTile>();
    }

    public override void OnKill()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            Item.NewItem(new EntitySource_TileBreak(Position.X, Position.Y, $"{nameof(Rosemary)}: {nameof(PolaroidTileEntity)}"), Position.ToWorldCoordinates(), Polaroid);
        }
        Polaroid = new Item();
    }

    public override void SaveData(TagCompound tag)
    {
        tag[nameof(Polaroid)] = ItemIO.Save(Polaroid);
    }

    public override void LoadData(TagCompound tag)
    {
        Polaroid = ItemIO.Load(tag.GetCompound(nameof(Polaroid)));
        Images.TrackImageDevelopment((PolaroidItem)Polaroid.ModItem);
    }

    public override void NetSend(BinaryWriter writer)
    {
        ItemIO.Send(Polaroid, writer);
    }

    public override void NetReceive(BinaryReader reader)
    {
        Polaroid = ItemIO.Receive(reader);
    }

    public override void Update()
    {
    }
}