using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace Rosemary.Common;

public static class ItemRarityExtension
{
    extension(Item item)
    {
        public Color GetRarityColor()
        {
            float multiplier = Main.mouseTextColor / 255f;
            if (item.IsAir || item.rare == ItemRarityID.White)
            {
                multiplier = 1f;
            }

            var rare = item.IsAir ? ItemRarityID.White : item.rare;

            Color color = ItemRarity.GetColor(rare);

            if (item.expert || item.rare == ItemRarityID.Expert)
            {
                color = Main.DiscoColor;
            }
            if (item.master || item.rare == ItemRarityID.Master)
            {
                color = new(255, (byte)(Main.masterColor * 200), 0);
            }

            return color * multiplier;
        }
    }
}