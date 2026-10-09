using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;

namespace Rosemary.Content.Misc;

public sealed class PolaroidItem : ModItem
{
    public override string Texture => Assets.Misc.Polaroid.KEY;

    public override string LocalizationCategory => "Content.Misc";

    public override bool CanStack(Item source) => false;

    public override void SetDefaults()
    {
        Item.width = 40;
        Item.height = 40;

        Item.value = Item.buyPrice(gold: 1);

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

        var color = Color.White;

        if (Images.TryRequestImage(ImageIdentifier, out var texture))
        {
            color = Color.Green;
            DrawPolaroid(texture, new Vector2(x, y));
        }

        ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, ImageIdentifier, new Vector2(x, y), color, 0f, Vector2.Zero, Vector2.One);

        return false;

        static void DrawPolaroid(Texture2D texture, Vector2 position)
        {
            var sb = Main.spriteBatch;

            sb.Draw(texture, position, Color.White);
        }
    }
}
