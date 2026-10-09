using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common;
using Rosemary.Core;
using Terraria;

namespace Rosemary.Vanity.Content;

public static class FlyeParticles
{
    public record struct Fish(Vector2 Position, Vector2 Velocity, float LifeTime) : IUpdatingParticle
    {
        public readonly float StartLifeTime = MathF.Max(1f, LifeTime);
        public readonly int Seed = Main.rand.Next(int.MaxValue); 
        bool IUpdatingParticle.Update()
        {
            var rand = new Rand(Seed);

            var progress = LifeTime / StartLifeTime;
            
            LifeTime--;

            Position += Velocity;

            Velocity = Velocity.RotatedBy(rand.NextFloat(0.1f, 0.25f) * MathF.Sin(LifeTime * rand.NextFloat(0.01f, 0.0f) + rand.NextFloat(MathF.PI * 2)) * progress);
            
            return LifeTime > 1f;
        }
    }
    
    public static UpdatingParticleHandler<Fish> Fishes { get; set; } = new(128);
    
    [ModSystemHooks.ClearWorld]
    private static void ClearParticles()
    {
        Fishes.Clear();
    }

    [ModSystemHooks.PostUpdateDusts]
    private static void UpdateParticlesPostDust()
    {
        Fishes.Update();
    }
    
    [ParticleLayer(ParticleLayers.BehindPlayers)]
    private static void DrawParticlesBehindPlayers(SpriteBatch sb)
    {
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
        {
            DrawFishes(sb, Main.screenPosition);
        }
        sb.End();
    }

    private static void DrawFishes(SpriteBatch sb, Vector2 offset)
    {
        if (Fishes.ActiveParticleCount <= 0)
        {
            return;
        }

        var texture = Assets.FlyeFish.Asset.Value;

        var origin = new Vector2(8f, 5f);

        foreach (var index in Fishes)
        {
            ref var fish = ref Fishes[index];
            
            var rand = new Rand(fish.Seed);

            var rotation = fish.Velocity.ToRotation();

            var progress = fish.LifeTime / fish.StartLifeTime;

            var opacity = MathHelper.Clamp(progress * 2, 0, 1);

            if (fish.LifeTime > fish.StartLifeTime - 6f)
                opacity *= 1f - (fish.LifeTime - fish.StartLifeTime + 5f) / 5f; 

            var scale = rand.NextFloat(0.8f, 1.2f);
            
            sb.Draw(texture, fish.Position - offset, null, Color.White * opacity, rotation, origin, scale, SpriteEffects.None, 0);
        }
    }
}