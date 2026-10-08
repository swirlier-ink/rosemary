using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Rosemary.Content.Misc;

public sealed class PolaroidItem : ModItem
{
    public override string Texture => Assets.Misc.Polaroid.KEY;

    public override string LocalizationCategory => "Content.Misc";

    public override void SetDefaults()
    {
        Item.width = 40;
        Item.height = 40;

        Item.value = Item.buyPrice(gold: 1);

        Item.rare = ItemRarityID.White;
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

        if (ImageSyncing.TryRequestImage(ImageIdentifier, out var texture))
        {
            DrawPolaroid(texture, new Vector2(x, y));
        }
        return false;

        static void DrawPolaroid(Texture2D texture, Vector2 position)
        {
            var sb = Main.spriteBatch;

            sb.Draw(texture, position, Color.White);
        }
    }
}
