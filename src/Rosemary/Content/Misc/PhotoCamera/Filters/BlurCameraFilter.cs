using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Content.Misc;

public sealed class BlurCameraFilter : ModItem, IPolaroidCameraFilter
{
    public override string Texture => Assets.Misc.PolaroidCameraFilters.BlurFilter.KEY;

    public override string LocalizationCategory => "Content.Misc.CameraFilters";

    bool IPolaroidCameraFilter.ApplyFilter(SpriteBatch sb, GraphicsDevice device, RenderTarget2D target, RenderTarget2D swap)
    {
        var horizBlur = Assets.Misc.PolaroidCameraFilters.BlurPolaroidShader.CreateHorizontalShader();
        var vertBlur = Assets.Misc.PolaroidCameraFilters.BlurPolaroidShader.CreateVerticalShader();

        var blurSize = new Vector2(16f / Images.BASE_RESOLUTION);

        device.SetRenderTarget(swap);
        device.Clear(Color.Transparent);

        sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null);
        {
            horizBlur.Parameters.BlurSize = blurSize;
            horizBlur.Apply();

            sb.Draw(target, Vector2.Zero, Color.LawnGreen);
        }
        sb.End();

        device.SetRenderTarget(target);
        device.Clear(Color.Transparent);

        sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null);
        {
            vertBlur.Parameters.BlurSize = blurSize;
            vertBlur.Apply();

            sb.Draw(swap, Vector2.Zero, Color.LawnGreen);
        }
        sb.End();

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
