using System;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Rosemary.Content.Misc;

public class PolaroidCamera : ModItem
{
    public override string Texture => Assets.Misc.PolaroidCamera.KEY;

    public override string LocalizationCategory => "Content.Misc";

    public override void Load()
    {
        if (Main.dedServ)
        {
            return;
        }

        On_PlayerDrawLayers.DrawPlayer_27_HeldItem += DrawPlayer_27_HeldItem_CachePlayer;
    }

    private static Player? currentPlayer;

    private static void DrawPlayer_27_HeldItem_CachePlayer(On_PlayerDrawLayers.orig_DrawPlayer_27_HeldItem orig, ref PlayerDrawSet drawInfo)
    {
        currentPlayer = drawInfo.drawPlayer;

        orig(ref drawInfo);
    }

    public override void SetDefaults()
    {
        Item.Size = new Vector2(32, 20);

        Item.value = Item.buyPrice(0, 10);

        Item.useTime = 3 * 60;
        Item.useAnimation = 3 * 60;

        Item.consumable = false;

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.holdStyle = ItemHoldStyleID.HoldHeavy;

        Item.rare = ItemRarityID.LightRed;

        Item.UseSound = SoundID.MenuTick;
    }

    public override Vector2? HoldoutOffset()
    {
        if (currentPlayer is null)
        {
            return new Vector2(-6, 2);
        }

        var off = currentPlayer.itemTime / (float)(3 * 60);
        off = MathF.Pow(off, 2f);
        off *= -4f;

        return new Vector2(-6, 2 + off);
    }

    public override bool? UseItem(Player player)
    {
        var id = Images.Capture();

        var item = new Item(ModContent.ItemType<PolaroidItem>());

        if (item.ModItem is PolaroidItem polaroid)
        {
            polaroid.ImageIdentifier = id;
        }

        // TODO: Delay until after some animation?
        player.QuickSpawnItem(player.GetSource_ItemUse(Item, nameof(PolaroidCamera)), item);

        return true;
    }
}