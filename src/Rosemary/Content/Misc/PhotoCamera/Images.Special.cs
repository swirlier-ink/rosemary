using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common;
using Rosemary.Core;
using System;
using MonoMod.Cil;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace Rosemary.Content.Misc;

public static partial class Images
{
    private static bool[] hiddenOnCamera = [];

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
        // This same effect could be achieved via GlobalNPC.Pre/PostDraw,
        // but I imagine it'd be more difficult to manage a scissor rectangle between Pre/PostDraw;
        // additionally makes drawing the NPC multiple times more annoying.
        On_Main.DrawNPCDirect += DrawNPCDirect_Exclusions;

        On_Main.DrawLensFlare += DrawLensFlare_Exclusions;
        IL_NextHorizonRenderer.DrawLensFlare += DrawLensFlare_Center;
    }

    private static bool drawingCameraFlare;

    private static void DrawLensFlare_Center(ILContext il)
    {
        var c = new ILCursor(il);

        var screenCenterIndex = VariableIndex.Invalid;

        c.GotoNext(
            MoveType.After,
            i => i.MatchCall<Main>($"get_{nameof(Main.ScreenSize)}"),
            i => i.MatchCall(typeof(Utils), nameof(Utils.ToVector2)),
            i => i.MatchLdcR4(2f),
            i => i.MatchCall<Vector2>("op_Division"),
            i => i.MatchStloc(out screenCenterIndex)
        );

        c.EmitLdloca(screenCenterIndex);
        c.EmitDelegate(
            static (ref Vector2 center) =>
            {
                if (drawingCameraFlare)
                {
                    center = GetCaptureArea().Center();
                }
            }
        );
    }

    private static void DrawLensFlare_Exclusions(On_Main.orig_DrawLensFlare orig)
    {
        if (!Main.ShouldDrawSurfaceBackground()
         || !Main.HorizonHelper.SunVisibilityEnabled
         || !CaptureRequested)
        {
            orig();

            return;
        }

        var device = Graphics.Device;
        var state = Main.LatestSurfaceBackgroundBeginner.rasterizerState;

        var sectors = GetOuterSectors();

        foreach (var rect in sectors)
        {
            using var _ = device.ScissorScope(rect, true, state);

            orig();
        }

        var frame = GetCaptureArea();

        var sunPosition = Main.LastCelestialBodyPosition * new Vector2(Main.screenWidth, Main.screenHeight);

        var sunSize = MathF.Pow(MathF.Abs(((float)Main.time / 54000f * 2f) - 1f), 2f);
        sunSize = 1.2f - (sunSize * 0.4f);
        sunSize *= 2f;

        var sunBounds = Utils.CenteredRectangle(sunPosition, TextureAssets.Sun.Value.Size() * sunSize);

        if (!frame.Intersects(sunBounds))
        {
            return;
        }

        using var __ = device.ScissorScope(frame, true, state);
        using var ___ = drawingCameraFlare.Override(true);

        orig();
    }

    private static void DrawNPCDirect_Exclusions(On_Main.orig_DrawNPCDirect orig, Main self, SpriteBatch sb, NPC npc, bool behindTiles, Vector2 screenPos)
    {
        if (!CaptureRequested
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

        var sectors = GetOuterSectors();

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

            using (device.ScissorScope(sector, true, state))
            {
                sb.Begin(in ss);
                {
                    orig(self, sb, npc, behindTiles, screenPos);
                }
                sb.End();
            }

            sb.Begin(in ss);
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

        var screenSize = new Vector2(Main.screenWidth, Main.screenHeight);

        DrawSectors();

        DrawEyes();

        return;

        void DrawEyes()
        {
            sb.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null);
            {
                var position = device.Viewport.Bounds.Center();
                position += bounds.TopLeft();
                position -= device.Viewport.Bounds.Size() - Vector2.Min(bounds.Size(), new Vector2(res));

                var capturePosition = GetCapturePosition() + new Vector2(ScaledResolution * 0.5f);
                capturePosition -= screenSize * 0.5f;
                {
                    capturePosition /= Main.GameZoomTarget;
                }
                capturePosition += screenSize * 0.5f;
                capturePosition += Main.screenPosition;

                for (var i = 0; i < Rand.Next(135, 200); i++)
                {
                    var off = RandomSquareEdge(Rand.Next(res * 0.8f, res * 1.1f));

                    var inWorldPosition = capturePosition + off;

                    var direction = inWorldPosition.DirectionTo(Main.LocalPlayer.Center);

                    var starPosition = position + off;

                    var eyeLid = Assets.Elk.Particles.ExpandingCircle.Asset.Value;

                    var scale = Rand.Next(0.05f, 1.2f);

                    var destSize = (new Vector2(16f, 36f) * scale) / eyeLid.Size();

                    var color = Color.White * 0.165f;
                    color.A = 0;

                    sb.Draw(eyeLid, starPosition, null, color, direction.ToRotation() + MathF.PiOver2, Origin.Center, destSize, SpriteEffects.None, 0f);

                    var starTexture = TextureAssets.Star[Rand.Next(TextureAssets.Star.Length)].Value;

                    sb.Draw(starTexture, starPosition + (direction * scale * 7f), null, Color.White with { A = 0 }, direction.ToRotation() + MathF.PiOver2, Origin.Center, scale, SpriteEffects.None, 0f);
                }
            }
            sb.End();

            return;

            static Vector2 RandomSquareEdge(float size)
            {
                var vector = Rand.NextSquare(-size, size);

                if (Rand.NextBool())
                {
                    vector.X = size * Rand.NextDirection();
                }
                else
                {
                    vector.Y = size * Rand.NextDirection();
                }

                return vector;
            }
        }

        void DrawSectors()
        {
            var veil = Assets.Misc.Camera.Veil.CreateVeilShader();

            veil.Parameters.Bounds = new Vector4(bounds.X, bounds.Y, MathF.Min(bounds.Right, res), MathF.Min(bounds.Bottom, res));
            veil.Parameters.FadeDistance = res * 0.5f;
            veil.Parameters.Time = Main.GlobalTimeWrappedHourly;

            veil.Parameters.Noise = new HlslSampler2D
            {
                Texture = Assets.Noise.MulticoloredNoise.Asset.ImmediateValue,
                Sampler = SamplerState.LinearWrap,
            };

            veil.Parameters.SwirlierNoises = new HlslSampler2D
            {
                Texture = Assets.Noise.SwirlyNoise.Asset.ImmediateValue,
                Sampler = SamplerState.LinearWrap,
            };

            veil.Apply();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, veil.Shader);
            {
                foreach (var (dest, src) in areas)
                {
                    if (dest.Width == 0 || dest.Height == 0)
                    {
                        continue;
                    }

                    sb.Draw(screen, dest, src, Color.White);
                }
            }
            sb.End();
        }
    }
}