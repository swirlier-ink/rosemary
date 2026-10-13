using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Rosemary.Content.Misc;

public class PolaroidCamera : ModItem, IPolaroidCamera
{
    public override string Texture => Assets.Misc.Camera.PolaroidCamera.KEY;

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

    public override void SetStaticDefaults()
    {
        ItemID.Sets.HasRightClickSound[Type] = false;
        ItemID.Sets.SortingPriorityToolsMisc[Type] = 14;
    }

    private const int use_time = 150;

    public override void SetDefaults()
    {
        Item.Size = new Vector2(32, 20);

        Item.value = Item.buyPrice(0, 10);

        Item.useTime = use_time;
        Item.useAnimation = use_time;

        Item.consumable = false;

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.holdStyle = ItemHoldStyleID.HoldHeavy;
        Item.useTurn = false;

        Item.rare = ItemRarityID.LightRed;

        Item.UseSound = Assets.Misc.Camera.Shoot.Asset;
    }

    public const int MAX_FILTERS = 8;

    public List<Item> Filters { set; get; } = [];

    public override bool CanStack(Item source)
    {
        var item = (PolaroidCamera)source.ModItem;

        return Filters.Count == 0 && item.Filters.Count == 0;
    }

    public override bool CanRightClick()
    {
        if (Main.mouseItem.IsAir)
        {
            return Filters.Count > 0;
        }

        return Filters.Count < MAX_FILTERS
            && Main.mouseItem.ModItem is IPolaroidCameraFilter;
    }

    public override void RightClick(Player player)
    {
        ItemInsertion.HandleRightClick(
            Item,
            player,
            // TODO: Is there perhaps a way to allow IPolaroidCameraFilter to apply to vanilla items?
            canInsert: i => Filters.Count < MAX_FILTERS && i.ModItem is IPolaroidCameraFilter,
            insertAction: (self, i) =>
            {
                ((PolaroidCamera)self.ModItem).Filters.Add(i);
                SoundEngine.PlaySound(Assets.Misc.Camera.InsertFilter.Asset);
            },
            removeAction: () =>
            {
                Main.mouseItem = Filters[^1].Clone();
                Filters.RemoveAt(Filters.Count - 1);
                SoundEngine.PlaySound(Assets.Misc.Camera.RemoveFilter.Asset);
            }
        );
    }

    public override ModItem Clone(Item newEntity)
    {
        var clone = (PolaroidCamera)base.Clone(newEntity);
        {
            clone.Filters = Filters.ToList();
        }
        return clone;
    }

    public override void SaveData(TagCompound tag)
    {
        tag[nameof(Filters)] = Filters.Select(ItemIO.Save).ToArray();
    }

    public override void LoadData(TagCompound tag)
    {
        Filters = tag.GetList<TagCompound>(nameof(Filters)).Select(ItemIO.Load).ToList();
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(Filters.Count);

        foreach (var item in Filters)
        {
            ItemIO.Send(item, writer);
        }
    }

    public override void NetReceive(BinaryReader reader)
    {
        var count = reader.ReadInt32();

        Filters.Clear();

        for (var i = 0; i < count; i++)
        {
            Filters.Add(ItemIO.Receive(reader));
        }
    }

    public override Vector2? HoldoutOffset()
    {
        if (currentPlayer is null)
        {
            return new Vector2(-6, 2);
        }

        var time = use_time - currentPlayer.itemAnimation;
        if (currentPlayer.itemAnimationMax == 0)
        {
            time = use_time;
        }

        var off = Utils.Remap(time, 0f, 45f, 1f, 0f);
        off = MathF.Pow(off, 1.3f);
        off *= -4f;

        return new Vector2(-6, 2 + off);
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        if (use_time - player.itemAnimation == 55)
        {
            SoundEngine.PlaySound(
                Assets.Misc.Camera.Print.Asset,
                player.Center,
                s =>
                {
                    s.Position = player.Center;
                    return true;
                }
            );
        }

        if (player.whoAmI != Main.myPlayer)
        {
            return;
        }

        if (player.itemAnimation > 1
         || queuedItem?.ModItem is not PolaroidItem polaroid)
        {
            return;
        }

        polaroid.OfCryptid = Images.CapturingCryptid;

        player.QuickSpawnItem(player.GetSource_ItemUse(Item, nameof(PolaroidCamera)), queuedItem);
        Images.TrackImageDevelopment(polaroid);

        queuedItem = null;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (Filters.Count <= 0)
        {
            return;
        }

        var index = tooltips.FindIndex(l => l.FullName == "Terraria/ItemName");

        if (index == -1)
        {
            return;
        }

        index++;

        tooltips.Insert(index, new TooltipLine(Mod, "Camera.FiltersHeader", Mods.Rosemary.Content.Misc.PolaroidCamera.FiltersHeader.GetTextValue()));
        index++;

        var count = 0;

        var colors = new[]
        {
            new Color(245, 116, 198),
            new Color(248, 244, 127),
            new Color(115, 209, 245),
        };

        foreach (var item in Filters)
        {
            var line = new TooltipLine(Mod, $"Camera.Filters: {count}", $"- {item.Name}");
            {
                line.Color = colors[count % 3];
            }
            tooltips.Insert(index + count, line);

            count++;
        }
    }

    private static Item? queuedItem;

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI != Main.myPlayer)
        {
            return true;
        }

        var id = Images.Capture(this);

        var item = new Item(ModContent.ItemType<PolaroidItem>());

        if (item.ModItem is PolaroidItem polaroid)
        {
            polaroid.ImageIdentifier = id;
        }

        queuedItem = item;

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

        var time = use_time - drawInfo.drawPlayer.itemAnimation;
        if (drawInfo.drawPlayer.itemAnimationMax == 0)
        {
            time = use_time;
        }

        var scale = Utils.Remap(time, 0f, 45f, 1f, 0f);
        scale = MathF.Pow(scale, 4f);

        var color = Color.White * scale;
        color.A = 0;

        var size = new Vector2(24f, 75) / texture.Size();
        size *= scale;

        DrawPrintAnimation(ref drawInfo);

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

        return;

        void DrawPrintAnimation(ref PlayerDrawSet drawInfo)
        {
            if (drawInfo.drawPlayer.itemAnimationMax == 0)
            {
                return;
            }

            var print = Assets.Misc.Camera.PolaroidCamera_PrintAnimation.Asset.Value;

            var frame = (int)Utils.Remap(time, 60, use_time, 0, 11);

            var source = print.Frame(1, 12, 0, frame);

            var origin = drawData.origin;
            origin.X += 4;

            var printData = drawData with
            {
                texture = print,
                sourceRect = source,
                origin = origin,
            };
            drawInfo.DrawDataCache.Add(printData);
        }
    }
}