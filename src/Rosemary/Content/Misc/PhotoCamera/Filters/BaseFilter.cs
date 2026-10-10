using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rosemary.Core;
using Terraria;

namespace Rosemary.Content.Misc;

public sealed class BaseFilter : IPolaroidCameraFilter
{
    bool IPolaroidCameraFilter.ApplyFilter(SpriteBatch sb, GraphicsDevice device, RenderTarget2D target, RenderTarget2D swap)
    {
        device.SetRenderTarget(swap);
        device.Clear(Color.Transparent);

        var noise = Assets.Noise.MulticoloredNoise.Asset.ImmediateValue;
        var shader = Assets.Misc.PolaroidCameraFilters.BasePolaroidShader.CreateBasePolaroidShader();

        shader.Parameters.Noise = new HlslSampler2D
        {
            Texture = noise,
            Sampler = SamplerState.LinearWrap,
        };
        shader.Parameters.Random = 0;
        shader.Parameters.Size = noise.Size() / 2f;

        shader.Apply();

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, shader.Shader);
        {
            sb.Draw(target, Vector2.Zero, Color.White);
        }
        sb.End();

        return true;
    }
}
