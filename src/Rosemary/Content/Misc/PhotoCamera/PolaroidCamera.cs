using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common;
using Terraria.ModLoader;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
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

        Item.useTime = 45;
        Item.useAnimation = 45;

        Item.consumable = false;

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.holdStyle = ItemHoldStyleID.HoldHeavy;
        Item.useTurn = false;

        Item.rare = ItemRarityID.LightRed;

        Item.UseSound = SoundID.MenuTick;
    }

    public override Vector2? HoldoutOffset()
    {
        if (currentPlayer is null)
        {
            return new Vector2(-6, 2);
        }

        var off = currentPlayer.itemAnimation / 45f;
        off = MathF.Pow(off, 1.3f);
        off *= -4f;

        return new Vector2(-6, 2 + off);
    }

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI != Main.myPlayer)
        {
            return true;
        }

        var id = Images.Capture();

        var item = new Item(ModContent.ItemType<PolaroidItem>());

        if (item.ModItem is PolaroidItem polaroid)
        {
            polaroid.ImageIdentifier = id;
            Images.TrackImageDevelopment(polaroid);
        }

        // TODO: Delay until after some animation?
        player.QuickSpawnItem(player.GetSource_ItemUse(Item, nameof(PolaroidCamera)), item);

        return true;
    }

    [GlobalItemHooks.PostModifyItemDraw]
    private static void PostModifyItemDraw(Item item, ref PlayerDrawSet drawInfo, DrawData drawData, DrawData? coloredDrawData, [OriginalName("glowmaskDrawData")] DrawData? glowMaskDrawData)
    {
        if (item.type != ModContent.ItemType<PolaroidCamera>())
        {
            return;
        }

        var scales = new Vector2(
            drawData.effect.HasFlag(SpriteEffects.FlipHorizontally) ? -1f : 1f,
            drawData.effect.HasFlag(SpriteEffects.FlipVertically) ? -1f : 1f
        );

        var flashPosition = drawData.position - (drawData.origin * drawData.scale);

        if (drawData.effect.HasFlag(SpriteEffects.FlipHorizontally))
        {
            flashPosition.X += (drawData.sourceRect?.Width ?? drawData.texture.Width);
        }
        if (drawData.effect.HasFlag(SpriteEffects.FlipVertically))
        {
            flashPosition.Y += (drawData.sourceRect?.Height ?? drawData.texture.Height);
        }

        flashPosition += new Vector2(24f, 4f) * scales;

        var texture = TextureAssets.Extra[ExtrasID.NinetyEight].Value;

        var scale = drawInfo.drawPlayer.itemAnimation / 45f;
        scale = MathF.Pow(scale, 4f);

        var color = Color.White * scale;
        color.A = 0;

        var size = new Vector2(24f, 75) / texture.Size();
        size *= scale;

        var data = new DrawData(
            texture,
            flashPosition,
            null,
            color,
            0f,
            texture.Size() * 0.5f,
            size,
            SpriteEffects.None
        );
        drawInfo.DrawDataCache.Add(data);

        size *= 0.75f;

        data = new DrawData(
            texture,
            flashPosition,
            null,
            color,
            MathF.PiOver2,
            texture.Size() * 0.5f,
            size,
            SpriteEffects.None
        );
        drawInfo.DrawDataCache.Add(data);
    }
}