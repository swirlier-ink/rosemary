using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using Rosemary.Common;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Rosemary.Content.Misc;

public sealed class PolaroidItem : ModItem
{
    public override string Texture => Assets.Misc.Polaroid.KEY;

    public override string LocalizationCategory => "Content.Misc";

    public override bool CanStack(Item source) => ImageIdentifier == ((PolaroidItem)source.ModItem).ImageIdentifier;

    public override bool CanResearch() => false;

    public override void Load()
    {
        if (Main.dedServ)
        {
            return;
        }

        MonoModHooks.Add(
            typeof(Main).GetMethod(
                nameof(Main.MouseText_DrawItemTooltip),
                BindingFlags.Instance | BindingFlags.NonPublic
            ),
            MouseText_DrawItemTooltip_HideTooltipBox
        );
    }

    private static void MouseText_DrawItemTooltip_HideTooltipBox(Action<Main, Main.MouseTextCache, int, byte, int, int> orig, Main self, Main.MouseTextCache info, int rare, byte diff, int x, int y)
    {
        using var _ = Main.SettingsEnabled_OpaqueBoxBehindTooltips.Cache();

        if (Main.HoverItem.type == ModContent.ItemType<PolaroidItem>())
        {
            Main.SettingsEnabled_OpaqueBoxBehindTooltips = false;
        }

        orig(self, info, rare, diff, x, y);
    }

    public override void SetDefaults()
    {
        Item.width = 40;
        Item.height = 40;

        Item.value = Item.buyPrice(gold: 1);

        // Purely to prevent burning
        Item.rare = ItemRarityID.Blue;
    }

    public string ImageIdentifier = string.Empty;

    public override void SaveData(TagCompound tag)
    {
        tag[nameof(ImageIdentifier)] = ImageIdentifier;
    }

    public override void LoadData(TagCompound tag)
    {
        ImageIdentifier = tag.Get<string>(nameof(ImageIdentifier));
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(ImageIdentifier);
    }

    public override void NetReceive(BinaryReader reader)
    {
        ImageIdentifier = reader.ReadString();
    }

    // TODO:
    // - Also make the image ONLY be requested if hovered in the inventory, not in world/chat
    public override bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        var sb = Main.spriteBatch;

        if (Images.TryRequestImage(ImageIdentifier, out var texture))
        {
            DrawPolaroid(texture, new Vector2(x, y), ImageIdentifier);
        }

        // ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, ImageIdentifier, new Vector2(x, y), color, 0f, Vector2.Zero, Vector2.One);

        return false;

        static void DrawPolaroid(Texture2D texture, Vector2 position, string identifier)
        {
            var sb = Main.spriteBatch;

            position -= new Vector2(4f);

            var screenSize = new Vector2(Main.screenWidth, Main.screenHeight);

            var polaroidBase = Assets.Misc.Polaroid_Tooltip.Asset.Value;

            var bottomRight = position + polaroidBase.Size();

            position -= Vector2.Clamp(bottomRight - screenSize, Vector2.Zero, screenSize);

            var bounds = new Rectangle((int)position.X + 18, (int)position.Y + 32, Images.BASE_RESOLUTION, Images.BASE_RESOLUTION);

            sb.Draw(polaroidBase, position, Color.White);
            
            sb.End(out var ss);

            var shader = Assets.Misc.Vignette.CreateVignetteShader();
            shader.Parameters.Intensity = 3;
            shader.Parameters.Power = 6;
            shader.Apply();
            sb.Begin(ss with { CustomEffect = shader.Shader });
            {
                sb.Draw(texture, bounds, Color.White);
                
                sb.Draw(texture, bounds, Color.Black * Images.BlackFade(identifier));
            }
            
            sb.Restart(ss);
        }
    }
}
