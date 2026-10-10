using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Rosemary.Common;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Rosemary.Content.Misc;

public sealed class DyeCameraFilter : ModItem, IPolaroidCameraFilter
{
    public override string Texture => Assets.Elk.TestItem.KEY;

    public override string LocalizationCategory => "Content.Misc.CameraFilters";

    public Item? Dye;
    public bool ValidDye(Item? dye) => dye is not null && GameShaders.Armor.GetShaderIdFromItemId(dye.type) > 0;

    bool IPolaroidCameraFilter.ApplyFilter(SpriteBatch sb, GraphicsDevice device, RenderTarget2D target, RenderTarget2D swap)
    {
        if (!ValidDye(Dye))
            return false;
        
        sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null);
        {
            device.SetRenderTarget(swap);
            device.Clear(Color.Transparent);

            var dye = GameShaders.Armor.GetShaderFromItemId(Dye!.type);
            dye.Apply(null);
            dye.uTargetPosition.SetValue(Vector2.Zero);
            dye.uSourceRect.SetValue(new Vector4(0, 0, target.Width, target.Height));
            dye.uLegacyArmorSourceRect.SetValue(new Vector4(0, 0, target.Width, target.Height));
            dye.uLegacyArmorSheetSize.SetValue(target.Size());;
            dye.uImageSize0.SetValue(target.Size());
            dye.Apply();
            
            sb.Draw(target, Vector2.Zero, Color.White);
        }
        sb.End();

        return true;
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
            return ValidDye(Dye);
        }

        return ValidDye(Main.mouseItem);
    }
    
    public override void RightClick(Player player)
    {
        Item.stack++;
        
        if (Main.mouseItem.IsAir && ValidDye(Dye))
        {
            Main.mouseItem = Dye!.Clone();
            Dye = null;
            
            Item.SetNameOverride(Mods.Rosemary.Content.Misc.CameraFilters.DyeCameraFilter.DisplayName.GetTextValue());

            return;
        }

        if (!ValidDye(Main.mouseItem))
        {
            return;
        }
        
        var item = Main.mouseItem.Clone();
        item.stack = 1;

        Dye = item;
        Item.SetNameOverride(Mods.Rosemary.Content.Misc.CameraFilters.DyeCameraFilter.DisplayName.GetTextValue() + $" ({Dye!.Name})");
        
        Main.mouseItem.stack--;
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
            ItemIO.Send(Dye, writer);
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
        var line = new TooltipLine(Mod, "DyeCameraFilter", $"- {Dye!.Name}");
        {
            line.Color = Main.MouseText_DrawItemTooltip_GetItemNameColor(Dye.rare, (byte)(Dye.master ? 2 : Dye.expert ? 1 : 0));
        }
        tooltips.Insert(index + 1, line);
    }
}
