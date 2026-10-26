using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Content.Flowers;

internal sealed class SunflowerGlobal : GlobalItem
{
    public override bool AppliesToEntity(Item item, bool lateInstantiation) =>
        item.type == ItemID.Sunflower;

    private static Asset<Texture2D> priorAsset = Asset<Texture2D>.Empty;

    public override void Load()
    {
        priorAsset = TextureAssets.Item[ItemID.Sunflower];
        TextureAssets.Item[ItemID.Sunflower] = Assets.Flowers.Sunflowers.SunflowerItem.Asset;
    }

    public override void Unload()
    {
        TextureAssets.Item[ItemID.Sunflower] = priorAsset;
    }

    public override void SetStaticDefaults()
    {
        // TODO: Better system for this, because this is stupid
        ItemID.Sets.DisableAutomaticPlaceableDrop[ItemID.Sunflower] = true;
    }

    public override void SetDefaults(Item item)
    {
        item.createTile = ModContent.TileType<SunflowerTile>();
    }
}
