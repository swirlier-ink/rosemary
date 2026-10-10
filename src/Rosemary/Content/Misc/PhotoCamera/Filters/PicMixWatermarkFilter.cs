using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Content.Misc;

public sealed class PicMixWatermarkFilter : ModItem, IPolaroidCameraFilter
{
    public override string Texture => Assets.Elk.TestItem.KEY;

    public override string LocalizationCategory => "Content.Misc.CameraFilters";

    bool IPolaroidCameraFilter.ApplyFilter(SpriteBatch sb, GraphicsDevice device, RenderTarget2D target, RenderTarget2D swap)
    {
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone);
        {
            var texture = Assets.Misc.PolaroidCameraFilters.PicMix.Asset.ImmediateValue;

            var position = device.Viewport.Bounds.BottomRight();

            sb.Draw(texture, position + new Vector2(1, 0), null, Color.White, 0f, Origin.BottomRight, 1f, SpriteEffects.None, 0f);
            sb.Draw(texture, position + new Vector2(-1, 0), null, Color.White, 0f, Origin.BottomRight, 1f, SpriteEffects.None, 0f);
            sb.Draw(texture, position + new Vector2(0, 1), null, Color.White, 0f, Origin.BottomRight, 1f, SpriteEffects.None, 0f);
            sb.Draw(texture, position + new Vector2(0, -1), null, Color.White, 0f, Origin.BottomRight, 1f, SpriteEffects.None, 0f);

            sb.Draw(texture, position, null, Color.Black, 0f, Origin.BottomRight, 1f, SpriteEffects.None, 0f);
        }
        sb.End();

        // We can safely return false as swap hasn't been drawn to last
        return false;
    }

    public override void SetDefaults()
    {
        Item.width = 40;
        Item.height = 40;

        Item.value = Item.buyPrice(gold: 1);

        // Purely to prevent burning
        Item.rare = ItemRarityID.Blue;
    }
}
