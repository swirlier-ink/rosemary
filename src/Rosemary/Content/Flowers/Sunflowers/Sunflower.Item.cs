using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Content.Flowers;

internal sealed class SunflowerGlobal : GlobalItem
{
    public override bool AppliesToEntity(Item item, bool lateInstantiation) =>
        item.type == ItemID.Sunflower;

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
