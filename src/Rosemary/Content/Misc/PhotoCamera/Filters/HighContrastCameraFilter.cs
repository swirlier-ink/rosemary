using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Content.Misc;

public sealed class HighContrastCameraFilter : ModItem, IPolaroidCameraFilter
{
    public override string Texture => Assets.Elk.TestItem.KEY;

    public override string LocalizationCategory => "Content.Misc.CameraFilters";

    bool IPolaroidCameraFilter.ApplyFilter(SpriteBatch sb, GraphicsDevice device, RenderTarget2D target, RenderTarget2D swap)
    {
        device.SetRenderTarget(swap);
        device.Clear(Color.Transparent);

        var shader = Assets.Misc.PolaroidCameraFilters.HighContrastPolaroidShader.CreateHighContrastPolaroidShader();

        shader.Parameters.Contrast = 1.7f;
        
        shader.Apply();

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, shader.Shader);
        {
            sb.Draw(target, Vector2.Zero, Color.White);
        }
        sb.End();

        return true;
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
