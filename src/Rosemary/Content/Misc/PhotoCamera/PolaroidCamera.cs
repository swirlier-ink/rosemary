using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;

namespace Rosemary.Content.Misc;

public class PolaroidCamera : ModItem
{
    public override string Texture => Assets.Misc.PolaroidCamera.KEY;

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.Size = new Vector2(32, 20);
        Item.consumable = false;
        Item.value = Item.buyPrice(0, 10);
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.holdStyle = ItemHoldStyleID.HoldHeavy;
        Item.rare = ItemRarityID.LightRed;
        Item.UseSound = SoundID.MenuTick;
    }

    public override Vector2? HoldoutOffset() => new Vector2(-6, 2);

    public override bool? UseItem(Player player)
    {
        var id = Images.Capture();

        var item = new Item(ModContent.ItemType<PolaroidItem>());

        if (item.ModItem is PolaroidItem polaroid)
        {
            polaroid.ImageIdentifier = id;
        }

        player.QuickSpawnItem(player.GetSource_ItemUse(Item, nameof(PolaroidCamera)), item);

        return true;
    }
}