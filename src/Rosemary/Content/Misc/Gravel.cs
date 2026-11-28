using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.GameContent.Drawing;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;


namespace Rosemary.Content.Misc;

public sealed class GravelItem : ModItem
{
    public override string Texture => Assets.Misc.Gravel.KEY;
    
    public override string LocalizationCategory => "Content.Misc";
    
    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 400;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<GravelTile>());
    }
}

public sealed class GravelTile : ModTile
{
    public override string Texture => Assets.Misc.GravelTile.KEY;

    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        TileID.Sets.CanBeDugByShovel[Type] = true;
        TileID.Sets.ChecksForMerge[Type] = true;
        Main.tileMergeDirt[Type] = true;
        Main.tileMerge[Type][TileID.Grass] = true;
        Main.tileMerge[Type][TileID.Stone] = true;
        
        AddMapEntry(new Color(130, 119, 107));

        RegisterItemDrop(ModContent.ItemType<GravelItem>());
    }
}
