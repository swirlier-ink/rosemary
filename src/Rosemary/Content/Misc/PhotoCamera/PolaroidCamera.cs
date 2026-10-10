using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Rosemary.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

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

        MonoModHooks.Modify(
            typeof(ItemLoader).GetMethod(
                nameof(ItemLoader.RightClick),
                BindingFlags.Public | BindingFlags.Static
            ),
            RightClick_DisableSound
        );
        IL_ItemSlot.RightClick += _ => { };
        IL_ItemSlot.GetGamepadInstructions_ItemArray_int_int += _ => { };
    }

    private static Player? currentPlayer;

    private static void DrawPlayer_27_HeldItem_CachePlayer(On_PlayerDrawLayers.orig_DrawPlayer_27_HeldItem orig, ref PlayerDrawSet drawInfo)
    {
        currentPlayer = drawInfo.drawPlayer;

        orig(ref drawInfo);
    }

    private static void RightClick_DisableSound(ILContext il)
    {
        var c = new ILCursor(il);

        var itemIndex = ParameterIndex.Invalid;

        var jumpPlaySoundLabel = c.DefineLabel();

        c.GotoNext(
            i => i.MatchLdarg(out itemIndex),
            i => i.MatchLdarg(out int _),
            i => i.MatchCall(typeof(ItemLoader), nameof(ItemLoader.RightClickCallHooks))
        );

        c.GotoNext(
            MoveType.Before,
            i => i.MatchLdcI4(7)
        );

        c.MoveAfterLabels();

        c.EmitLdarg(itemIndex);
        c.EmitDelegate(
            static (Item item) =>
            {
                return item.type == ModContent.ItemType<PolaroidCamera>();
            }
        );
        c.EmitBrtrue(jumpPlaySoundLabel);

        c.GotoNext(
            MoveType.After,
            i => i.MatchCall(typeof(SoundEngine), nameof(SoundEngine.PlaySound)),
            i => i.MatchPop()
        );

        c.MarkLabel(jumpPlaySoundLabel);
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

    public const int MAX_FILTERS = 8;

    public List<Item> Filters = [];

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
        Item.stack++;

        if (Main.mouseItem.IsAir && Filters.Count > 0)
        {
            Main.mouseItem = Filters[^1].Clone();
            Filters.RemoveAt(Filters.Count - 1);

            // play some sound

            return;
        }

        if (Filters.Count >= MAX_FILTERS
         || Main.mouseItem.ModItem is not IPolaroidCameraFilter)
        {
            return;
        }

        var item = Main.mouseItem.Clone();
        item.stack = 1;

        Main.mouseItem.stack--;
        
        if (Item.stack > 2)
        {
            Item.stack--;
            
            var newCamera = new Item(Type);
            {
                var modItem = (PolaroidCamera)newCamera.ModItem;
                modItem.Filters.Add(item);
            }
            if (Main.mouseItem.stack > 0)
                player.QuickSpawnItem(player.GetItemSource_Item(Item), newCamera);
            else
                Main.mouseItem = newCamera;
        }
        else 
        {
            Filters.Add(item);
        }
        
        // play some other sound
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

        var off = currentPlayer.itemAnimation / 45f;
        off = MathF.Pow(off, 1.3f);
        off *= -4f;

        return new Vector2(-6, 2 + off);
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

        tooltips.Insert(index, new TooltipLine(Mod, "PolaroidCameraFiltersHeader", Mods.Rosemary.Content.Misc.PolaroidCamera.FiltersHeader.GetTextValue()));
        index++;

        var count = 0;

        foreach (var item in Filters)
        {
            var line = new TooltipLine(Mod, $"PolaroidCameraFilters: {count}", $"- {item.Name}");
            {
                line.Color = (count % 2 == 0) ? new Color(255, 154, 86) : new Color(211, 98, 164);
            }
            tooltips.Insert(index + count, line);

            count++;
        }
    }

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