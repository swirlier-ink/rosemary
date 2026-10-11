using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Rosemary.Common;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Rosemary.Content.Misc;

public sealed class DyeCameraFilter : ModItem, IPolaroidCameraFilter
{
    public override string Texture => Assets.Misc.PolaroidCameraFilters.DyeFilter_Empty.KEY;

    public override string LocalizationCategory => "Content.Misc.CameraFilters";

    public Item? Dye;

    public static bool ValidDye(Item? dye) => dye is not null && GameShaders.Armor.GetShaderIdFromItemId(dye.type) > 0;

    bool IPolaroidCameraFilter.ApplyFilter(SpriteBatch sb, GraphicsDevice device, RenderTarget2D target, RenderTarget2D swap)
    {
        if (Dye is null
         || !ValidDye(Dye))
        {
            return false;
        }

        device.SetRenderTarget(swap);
        device.Clear(Color.Transparent);

        sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null);
        {
            var dye = GameShaders.Armor.GetShaderFromItemId(Dye.type);

            var data = new DrawData(
                target,
                Vector2.Zero,
                Color.White
            );

            dye.Apply(null, data);

            data.Draw(sb);
        }
        sb.End();

        return true;
    }

    public override void SetStaticDefaults()
    {
        ItemID.Sets.HasRightClickSound[Type] = false;
    }

    public override void SetDefaults()
    {
        Item.width = 40;
        Item.height = 40;

        Item.value = Item.buyPrice(gold: 1);

        // Purely to prevent burning
        Item.rare = ItemRarityID.Blue;
    }

    public override bool CanRightClick()
    {
        if (Main.mouseItem.IsAir)
        {
            return Dye is not null;
        }

        return ValidDye(Main.mouseItem);
    }

    public override bool CanStack(Item source)
    {
        var item = (DyeCameraFilter)source.ModItem;

        return item.Dye is null && Dye is null;
    }

    public override void RightClick(Player player)
    {
        ItemInsertion.HandleRightClick(
            Item,
            player,
            // TODO: Is there perhaps a way to allow IPolaroidCameraFilter to apply to vanilla items?
            canInsert: ValidDye,
            insertAction: (self, i) =>
            {
                var modItem = (DyeCameraFilter)self.ModItem;
                modItem.Dye = i;

                modItem.SetName(i);

                SoundEngine.PlaySound(in SoundID.Unlock);
            },
            removeAction: () =>
            {
                Main.mouseItem = Dye!.Clone();
                Dye = null;

                SetName();

                SoundEngine.PlaySound(in SoundID.Unlock);
            }
        );
    }

    private void SetName(Item? item = null)
    {
        Item.SetNameOverride(Mods.Rosemary.Content.Misc.CameraFilters.DyeCameraFilter.DisplayName.GetTextValue() + (item is null ? string.Empty : $" ({item.Name})"));
    }
    
    public override ModItem Clone(Item newEntity)
    {
        var clone = (DyeCameraFilter)base.Clone(newEntity);
        {
            clone.Dye = Dye;
        }
        return clone;
    }
    
    public override void SaveData(TagCompound tag)
    {
        if (Dye is null)
        {
            tag["isNull"] = true;
            return;
        }
        
        tag["isNull"] = false;
        tag[nameof(Dye)] = ItemIO.Save(Dye);
    }

    public override void LoadData(TagCompound tag)
    {
        var isNull = tag.GetBool("isNull");
        
        Dye = isNull ? null : ItemIO.Load(tag.GetCompound(nameof(Dye)));
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(Dye is null);
        
        if (Dye is not null)
        {
            ItemIO.Send(Dye, writer);
        }
    }

    public override void NetReceive(BinaryReader reader)
    {
        var isNull = reader.ReadBoolean();

        Dye = isNull ? null : ItemIO.Receive(reader);
    }
    
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (!ValidDye(Dye))
        {
            return;
        }

        var index = tooltips.FindIndex(l => l.FullName == "Terraria/ItemName");

        if (index == -1)
        {
            return;
        }

        tooltips[index].Text = Mods.Rosemary.Content.Misc.CameraFilters.DyeCameraFilter.DisplayName.GetTextValue();

        index++;

        // Potentially scary, but should allow modded rarities to apply correctly
        var line = new TooltipLine("ItemName", $"- {Dye!.Name}");
        {
            line.Color = Main.MouseText_DrawItemTooltip_GetItemNameColor(Dye.rare, (byte)(Dye.master ? 2 : Dye.expert ? 1 : 0));
        }
        tooltips.Insert(index + 1, line);
    }

    public override bool PreDrawInInventory(SpriteBatch sb, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (Dye is null)
        {
            return true;
        }

        var definition = ItemID.Sets.DyeDefinitions[Dye.type];

        sb.End(out var ss);
        sb.Begin(ss with { SortMode = SpriteSortMode.Immediate });
        {
            var outlineTexture = Assets.Misc.PolaroidCameraFilters.DyeFilter_Outline.Asset.Value;

            var outlineFrame = outlineTexture.Frame(11, 1, (int)(definition?.LidType ?? 0));
            outlineFrame.Width -= 2;

            sb.Draw(outlineTexture, position, outlineFrame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);

            var lensTexture = Assets.Misc.PolaroidCameraFilters.DyeFilter_Lens.Asset.Value;

            var dye = GameShaders.Armor.GetShaderFromItemId(Dye.type);

            var data = new DrawData(
                lensTexture,
                position,
                null,
                drawColor,
                0f, origin,
                scale,
                SpriteEffects.None
            );

            dye.Apply(null, data);
            {
                data.Draw(sb);
            }
            sb.spriteEffect.CurrentTechnique.Passes[0].Apply();

            if (definition?.Overlays.HasFlag(DyeOverlayType.Animated) is true)
            {
                var overlayTexture = Assets.Misc.PolaroidCameraFilters.DyeFilter_Overlay.Asset.Value;

                sb.Draw(overlayTexture, position, null, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }
        sb.Restart(in ss);

        return false;
    }

    public override bool PreDrawInWorld(WorldItem item, SpriteBatch sb, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        if (Dye is null)
        {
            return true;
        }

        Main.instance.DrawItem_GetBasics(item.inner, whoAmI, out _, out var frame, out _);

        var origin = frame.Size() * 0.5f;
        var off = new Vector2((item.width * 0.5f) - origin.X, item.height - frame.Height);
        var position = (item.position + origin + off);

        var definition = ItemID.Sets.DyeDefinitions[Dye.type];

        sb.End(out var ss);
        sb.Begin(ss with { SortMode = SpriteSortMode.Immediate });
        {
            var outlineTexture = Assets.Misc.PolaroidCameraFilters.DyeFilter_Outline.Asset.Value;

            var outlineFrame = outlineTexture.Frame(11, 1, (int)(definition?.LidType ?? 0));
            outlineFrame.Width -= 2;

            sb.Draw(outlineTexture, position - Main.screenPosition, outlineFrame, alphaColor, rotation, origin, scale, SpriteEffects.None, 0f);

            var lensTexture = Assets.Misc.PolaroidCameraFilters.DyeFilter_Lens.Asset.Value;

            var dye = GameShaders.Armor.GetShaderFromItemId(Dye.type);

            var data = new DrawData(
                lensTexture,
                position - Main.screenPosition,
                null,
                alphaColor,
                rotation,
                origin,
                scale,
                SpriteEffects.None
            );

            dye.Apply(item, data);
            {
                data.Draw(sb);
            }
            sb.spriteEffect.CurrentTechnique.Passes[0].Apply();

            if (definition?.Overlays.HasFlag(DyeOverlayType.Animated) is true)
            {
                var overlayTexture = Assets.Misc.PolaroidCameraFilters.DyeFilter_Overlay.Asset.Value;

                sb.Draw(overlayTexture, position - Main.screenPosition, null, alphaColor, rotation, origin, scale, SpriteEffects.None, 0f);
            }
        }
        sb.Restart(in ss);

        return false;
    }
}
