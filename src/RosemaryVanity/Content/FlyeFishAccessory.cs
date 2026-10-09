using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Vanity.Content;

public class FlyeFishAccessory : ModItem
{
    public override string Texture => Assets.FlyeFish.KEY;

    public override void SetDefaults()
    {
        Item.DefaultToAccessory();
        Item.vanity = true;
        Item.rare = ItemRarityID.Cyan;
    }

    public override void UpdateVisibleAccessory(Player player, bool hideVisual)
    {
        base.UpdateVisibleAccessory(player, hideVisual);

        if (!Main.dedServ && Main.GameUpdateCount % 5 == 0 && player.velocity.Length() > 0.2f)
            FlyeParticles.Fishes += new FlyeParticles.Fish(player.Center + new Vector2(-player.direction * 2, player.height / 2f - 4).RotatedBy(player.fullRotation), new Vector2(-player.direction * 2, player.velocity.Y * -0.1f), 100);
    }
}