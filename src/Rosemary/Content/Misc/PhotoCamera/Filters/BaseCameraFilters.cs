using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common;
using Rosemary.Core;
using Terraria;

namespace Rosemary.Content.Misc;

public sealed class BaseMotionBlurFilter : IPolaroidCameraFilter
{
    bool IPolaroidCameraFilter.ApplyFilter(SpriteBatch sb, GraphicsDevice device, RenderTarget2D target, RenderTarget2D swap)
    {
        var blur = Assets.Misc.Camera.Filters.BlurPolaroidShader.CreateDirectionalShader();

        var speed = MathF.Min(Main.LocalPlayer.velocity.Length() / 12f, 1f);

        if (speed <= 0f)
        {
            return false;
        }

        var dir = Main.LocalPlayer.velocity.WithLength(speed);

        var blurSize = new Vector2(48f / Images.BASE_RESOLUTION) * dir;

        device.SetRenderTarget(swap);
        device.Clear(Color.Transparent);

        sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null);
        {
            blur.Parameters.BlurSize = blurSize;
            blur.Apply();

            sb.Draw(target, Vector2.Zero, Color.White);
        }
        sb.End();

        return true;
    }
}

public sealed class BaseBloomFilter : IPolaroidCameraFilter
{
    bool IPolaroidCameraFilter.ApplyFilter(SpriteBatch sb, GraphicsDevice device, RenderTarget2D target, RenderTarget2D swap)
    {
        using var lease = RenderTargetPool.Shared.Rent(device, Images.BASE_RESOLUTION, Images.BASE_RESOLUTION);

        using (lease.Scope(clearColor: Color.Transparent))
        {
            DrawContrast();

            DrawBlur();
        }

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null);
        {
            var color = Color.White * 0.25f;
            color.A = 0;

            sb.Draw(lease.Target, Vector2.Zero, color);
        }
        sb.End();

        return false;

        void DrawContrast()
        {
            var shader = Assets.Misc.Camera.Filters.HighContrastPolaroidShader.CreateHighContrastPolaroidShader();

            shader.Parameters.Contrast = 3f;

            shader.Apply();

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, shader.Shader);
            {
                sb.Draw(target, Vector2.Zero, Color.White);
            }
            sb.End();
        }

        void DrawBlur()
        {
            var horizBlur = Assets.Misc.Camera.Filters.BlurPolaroidShader.CreateHorizontalShader();
            var vertBlur = Assets.Misc.Camera.Filters.BlurPolaroidShader.CreateVerticalShader();

            var blurSize = new Vector2(8f / Images.BASE_RESOLUTION);

            device.SetRenderTarget(swap);
            device.Clear(Color.Transparent);

            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null);
            {
                horizBlur.Parameters.BlurSize = blurSize;
                horizBlur.Apply();

                sb.Draw(lease.Target, Vector2.Zero, Color.White);
            }
            sb.End();

            device.SetRenderTarget(lease.Target);
            device.Clear(Color.Transparent);

            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null);
            {
                vertBlur.Parameters.BlurSize = blurSize;
                vertBlur.Apply();

                sb.Draw(swap, Vector2.Zero, Color.White);
            }
            sb.End();
        }
    }
}

public sealed class BaseNoiseFilter : IPolaroidCameraFilter
{
    bool IPolaroidCameraFilter.ApplyFilter(SpriteBatch sb, GraphicsDevice device, RenderTarget2D target, RenderTarget2D swap)
    {
        device.SetRenderTarget(swap);
        device.Clear(Color.Transparent);

        var noise = Assets.Noise.MulticoloredNoise.Asset.ImmediateValue;
        var shader = Assets.Misc.Camera.Filters.BasePolaroidShader.CreateBasePolaroidShader();

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
