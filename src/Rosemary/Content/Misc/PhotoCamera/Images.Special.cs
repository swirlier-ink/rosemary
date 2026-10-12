using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common;
using Rosemary.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Content.Misc;

public static partial class Images
{
    private static bool[] hiddenOnCamera = [];

    private static Mod Mod => ModContent.GetInstance<ModImpl>();

    [ModSystemHooks.ResizeArrays]
    private static void ResizeArrays()
    {
        hiddenOnCamera = CreateSet(nameof(hiddenOnCamera), false);

        NPCID.Sets.HiddenOnCamera[NPCID.Vampire] = true;
        NPCID.Sets.HiddenOnCamera[NPCID.VampireBat] = true;

        return;

        static T[] CreateSet<T>(string name, T defaultState)
        {
            return NPCID.Sets.Factory.CreateNamedSet(Mod, name)
                         .RegisterCustomSet(defaultState);
        }
    }

    extension(NPCID.Sets)
    {
        public static bool[] HiddenOnCamera => hiddenOnCamera;
    }

    [OnLoad]
    private static void Load_Special()
    {
        // This same effect could be achieved via GlobalNPC or s
        On_Main.DrawNPCDirect += DrawNPCDirect_Exclusions;
    }

    private static void DrawNPCDirect_Exclusions(On_Main.orig_DrawNPCDirect orig, Main self, SpriteBatch sb, NPC npc, bool behindTiles, Vector2 screenPos)
    {
        if (!captureRequested
         || !NPCID.Sets.HiddenOnCamera[npc.type])
        {
            orig(self, sb, npc, behindTiles, screenPos);
            return;
        }

        var frame = GetCaptureArea();

        var worldFrame = frame;
        worldFrame.X += (int)Main.screenPosition.X;
        worldFrame.Y += (int)Main.screenPosition.Y;

        var npcBounds = npc.Hitbox;

        // TODO: See if scale is correctly accounted for
        var length = (int)npc.Hitbox.Size().Length();

        npcBounds.Inflate(((length - npcBounds.Width) / 2), ((length - npcBounds.Height) / 2));

        if (!npcBounds.Intersects(worldFrame))
        {
            orig(self, sb, npc, behindTiles, screenPos);

            return;
        }

        // Contained entirely within frame, ignore
        if (worldFrame.Contains(npcBounds))
        {
            return;
        }

        var device = self.GraphicsDevice;
        var viewport = device.Viewport.Bounds;

        var sectors = new[]
        {
            // Left
            new Rectangle(0, frame.Y, frame.X, frame.Height),
            // Right
            new Rectangle(frame.Right, frame.Y, viewport.Width - frame.Right, frame.Height),
            // Up (spans whole width)
            new Rectangle(0, 0, viewport.Width, frame.Y),
            // Down (spans whole width)
            new Rectangle(0, frame.Bottom, viewport.Width, viewport.Height - frame.Bottom),
        };

        var npcScreen = npcBounds;
        npcScreen.X -= (int)Main.screenPosition.X;
        npcScreen.Y -= (int)Main.screenPosition.Y;

        foreach (var rect in sectors)
        {
            if (npcScreen.Intersects(rect))
            {
                DrawToSection(rect);
            }
        }

        return;

        void DrawToSection(Rectangle sector)
        {
            sb.End(out var ss);

            var state = ss.RasterizerState;

            var prior = state.ScissorTestEnable;
            state.ScissorTestEnable = true;

            var priorRect = device.ScissorRectangle;
            device.ScissorRectangle = Rectangle.Intersect(priorRect, sector);

            sb.Begin(ss with { RasterizerState = state });
            {
                orig(self, sb, npc, behindTiles, screenPos);
            }
            sb.End();

            device.ScissorRectangle = priorRect;
            state.ScissorTestEnable = prior;

            sb.Begin(ss with { RasterizerState = state });
        }
    }

    private static void DrawBarrier(SpriteBatch sb, GraphicsDevice device, Rectangle bounds, Rectangle source, Texture2D screen)
    {
        const int res = BASE_RESOLUTION;

        var areas = new[]
        {
            (new Rectangle(0, bounds.Y, bounds.X, bounds.Height), source with { Width = 1 }),
            (new Rectangle(bounds.Right, bounds.Y, res - bounds.Right, bounds.Height), source with { X = source.Right - 1, Width = 1 }),

            (new Rectangle(bounds.X, 0, bounds.Width, bounds.Y), source with { Height = 1 }),
            (new Rectangle(bounds.X, bounds.Bottom, bounds.Width, res - bounds.Bottom), source with { Y = source.Bottom - 1, Height = 1 }),

            // Corners
            (new Rectangle(0, 0, bounds.X, bounds.Y), source with { Width = 1, Height = 1 }),
            (new Rectangle(bounds.Right, 0, res - bounds.Right, bounds.Y), source with { X = source.Right - 1, Width = 1, Height = 1 }),
            (new Rectangle(0, bounds.Bottom, bounds.X, res - bounds.Bottom), source with { Y = source.Bottom - 1, Width = 1, Height = 1 }),
            (new Rectangle(bounds.Right, bounds.Bottom, res - bounds.Right, res - bounds.Bottom), source with { X = source.Right - 1, Y = source.Bottom - 1, Width = 1, Height = 1 }),
        };

        var noise = Assets.Noise.MulticoloredNoise.Asset.ImmediateValue;

        var veil = Assets.Misc.Veil.CreateVeilShader();

        veil.Parameters.Bounds = new Vector4(bounds.X, bounds.Y, MathF.Min(bounds.Right, BASE_RESOLUTION), MathF.Min(bounds.Bottom, BASE_RESOLUTION));
        veil.Parameters.Time = Main.GlobalTimeWrappedHourly;

        veil.Parameters.Noise = new HlslSampler2D
        {
            Texture = noise,
            Sampler = SamplerState.LinearWrap,
        };

        veil.Apply();
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, veil.Shader);
        {
            foreach (var (dest, src) in areas)
            {
                sb.Draw(screen, dest, src, Color.White);
            }
        }
        sb.End();
    }
}