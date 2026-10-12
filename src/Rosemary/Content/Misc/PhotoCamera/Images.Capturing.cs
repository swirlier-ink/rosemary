using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Rosemary.Common;
using Rosemary.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Rosemary.Content.Misc;

public static partial class Images
{
    public const int BASE_RESOLUTION = 256;

    private static int ScaledResolution => (int)(BASE_RESOLUTION * MathF.Clamp(Main.GameZoomTarget, 1f, 2f));
    
    private static bool captureRequested;
    private static float captureVfx;

    private static string identifierToCreate = string.Empty;

    private static readonly List<IPolaroidCameraFilter> base_filters = [ new BaseCameraFilter() ];

    private static List<IPolaroidCameraFilter> filters = [];

    public static string Capture(IPolaroidCamera item)
    {
        var captureFilters = new List<IPolaroidCameraFilter>();

        foreach (var filter in item.Filters)
        {
            if (filter.ModItem is IPolaroidCameraFilter f)
            {
                captureFilters.Add(f);
            }
        }

        return Capture(captureFilters);
    }

    public static string Capture(List<IPolaroidCameraFilter> captureFilters)
    {
        captureVfx = 1f;
        captureRequested = true;

        identifierToCreate = CreateIdentifier();

        filters = base_filters.Concat(captureFilters).ToList();

        return identifierToCreate;
    }

    [OnLoad(Side = ModSide.Client)]
    private static void Load_Capturing()
    {
        On_FilterManager.EndCapture_RenderTarget2D_RenderTarget2D_RenderTarget2D_Vector2_Vector2_Vector2 += HandleCapture;
        On_Main.DrawInterface_36_Cursor += DrawInterface_36_Cursor_HideWhilstUsingCamera;

        On_Main.DrawInterface_41_InterfaceLogic4 += DrawInterface_41_InterfaceLogic4_CacheTooltipState;
    }

    private static bool oldTooltipVisible;

    private static void DrawInterface_41_InterfaceLogic4_CacheTooltipState(On_Main.orig_DrawInterface_41_InterfaceLogic4 orig)
    {
        orig();

        oldTooltipVisible = Main.instance._mouseTextCache.isValid
                         || !Main.MouseDisplayItem.IsAir
                         || Main.LocalPlayer.cursorItemIconEnabled;
    }

    private static void DrawInterface_36_Cursor_HideWhilstUsingCamera(On_Main.orig_DrawInterface_36_Cursor orig)
    {
        var alive = Main.LocalPlayer is { noItems: false, CCed: false, dead: false };

        if (alive
         && Main.LocalPlayer.HeldItem.ModItem is IPolaroidCamera
         && !Main.LocalPlayer.lastMouseInterface
         && !Main.gamePaused
         && FocusHelper.GameplayActive
         && !oldTooltipVisible)
        {
            return;
        }

        orig();
    }

    [ModSystemHooks.PostUpdateEverything]
    private static void PostUpdate_Capturing()
    {
        captureVfx = MathF.Max(0, captureVfx - 0.1f);
    }

    [ModSystemHooks.RequiresScreenTarget]
    private static bool RequiresScreenTarget() => captureRequested;

    public static Rectangle GetCaptureArea()
    {
        using var _ = PlayerInput.ZoomScope(ZoomScaleType.Unscaled);

        var position = GetImagePosition();
        var frame = new Rectangle((int)position.X, (int)position.Y, ScaledResolution, ScaledResolution);

        return frame;

        static Vector2 GetImagePosition()
        {
            var worldPlayArea = WorldUtils.GetWorldPlayArea();

            var size = new Vector2(ScaledResolution) * 0.5f;

            var topLeft = size;
            var bottomRight = new Vector2(Main.screenWidth, Main.screenHeight) - size;

            if (Main.screenPosition.X <= worldPlayArea.X)
            {
                topLeft.X -= 999f;
            }
            if (Main.screenPosition.Y <= worldPlayArea.Y)
            {
                topLeft.Y -= 999f;
            }
            // TODO: Account for zoom nicely
            if (Main.screenPosition.X + Main.screenWidth >= worldPlayArea.X + worldPlayArea.Width)
            {
                bottomRight.X += 999f;
            }
            if (Main.screenPosition.Y + Main.screenHeight >= worldPlayArea.Y + worldPlayArea.Height)
            {
                bottomRight.Y += 999f;
            }

            var position = Vector2.Clamp(Main.MouseScreen, topLeft, bottomRight) - size;

            return position;
        }
    }

    private static void HandleCapture(On_FilterManager.orig_EndCapture_RenderTarget2D_RenderTarget2D_RenderTarget2D_Vector2_Vector2_Vector2 orig, FilterManager self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Vector2 screenSize, Vector2 sceneSize, Vector2 sceneOffset)
    {
        orig(self, finalTexture, screenTarget1, screenTarget2, screenSize, sceneSize, sceneOffset);

        if (Main.gameMenu)
        {
            return;
        }

        var sb = Main.spriteBatch;

        var device = Main.graphics.GraphicsDevice;

        using var _ = PlayerInput.ZoomScope(ZoomScaleType.Unscaled);

        var frame = GetCaptureArea();

        DrawCameraOverlay(sb, frame);

        if (!captureRequested
         || string.IsNullOrEmpty(identifierToCreate))
        {
            captureRequested = false;
            return;
        }

        using var lease = RenderTargetPool.Shared.Rent(device, BASE_RESOLUTION, BASE_RESOLUTION);
        using var swapLease = RenderTargetPool.Shared.Rent(device, BASE_RESOLUTION, BASE_RESOLUTION);

        var target = lease.Target;
        var swap = swapLease.Target;

        DrawImage();

        var id = identifierToCreate;

        using var stream = new FileStream(GetImagePath(id), FileMode.Create);
        target.SaveAsJpeg(stream, ScaledResolution, ScaledResolution);
        local_identifiers.Add(id);

        captureRequested = false;

        return;

        void DrawImage()
        {
            using var _ = target.Scope(clearColor: Color.Transparent);

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone);
            {
                var screen = Main.finalScreenTarget!;

                var dest = Main.graphics.GraphicsDevice.Viewport.Bounds;

                var zoomFactor = MathF.Clamp(Main.GameZoomTarget, 1f, 2f);

                dest.X += Math.Max(-(int)(frame.X / zoomFactor), 0);
                dest.Y += Math.Max(-(int)(frame.Y / zoomFactor), 0);

                frame.X = Math.Max(frame.X, 0);
                frame.Y = Math.Max(frame.Y, 0);

                dest.Width -= (int)((frame.Right - screen.Width) / zoomFactor);
                dest.Height -= (int)((frame.Bottom - screen.Height) / zoomFactor);

                frame.Width -= frame.Right - screen.Width;
                frame.Height -= frame.Bottom - screen.Height;

                sb.Draw(screen, dest, frame, Color.White);
            }
            sb.End();

            foreach (var filter in filters)
            {
                if (filter.ApplyFilter(sb, device, target, swap))
                {
                    Utils.Swap(ref target, ref swap);
                }
            }
        }
    }
    
    private static void DrawCameraOverlay(SpriteBatch sb, Rectangle frame)
    {
        var player = Main.LocalPlayer;

        var alive = player is { noItems: false, CCed: false, dead: false };

        var texture = TextureAssets.MagicPixel.Value;

        if (!alive
         || player.HeldItem.ModItem is not IPolaroidCamera
         || player.lastMouseInterface
         || Main.gamePaused
         || !FocusHelper.GameplayActive)
        {
            return;
        }

        var fade = (player.itemAnimationMax <= 0 ? 0f : player.itemAnimation / (float)player.itemAnimationMax);
        fade = Utils.Remap(fade, 0f, 1f, 1f, 0.6f);

        var color = Color.White * fade;
        color.A = byte.MaxValue;

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        {
            var circle = Assets.Misc.PolaroidCameraCircle.Asset.Value;
            var origin = circle.Size() * 0.5f;

            var circleColor = color;

            if (oldTooltipVisible)
            {
                circleColor *= 0.35f;
            }

            sb.Draw(circle, Main.MouseScreen, null, circleColor, 0f, origin, 1f, SpriteEffects.None, 0f);

            DrawFrame();

            sb.Draw(texture, frame, Color.White * captureVfx);
        }
        sb.End();

        return;

        void DrawFrame()
        {
            const float off = 10f;

            var pulse = (1f - MathF.Pow(1f - captureVfx, 3f)) * off;

            var size = frame;

            size.Inflate((int)(-off + pulse), (int)(-off + pulse));

            sb.Draw(texture, size with { Width = 60, Height = 2 }, null, color, 0f, Origin.TopLeft, SpriteEffects.None, 0f);
            sb.Draw(texture, size with { Width = 2, Height = 60 }, null, color, 0f, Origin.TopLeft, SpriteEffects.None, 0f);

            sb.Draw(texture, size with { X = size.Right, Width = 60, Height = 2 }, null, color, 0f, Origin.TopRight, SpriteEffects.None, 0f);
            sb.Draw(texture, size with { X = size.Right, Width = 2, Height = 60 }, null, color, 0f, Origin.TopRight, SpriteEffects.None, 0f);

            sb.Draw(texture, size with { Y = size.Bottom, Width = 60, Height = 2 }, null, color, 0f, Origin.BottomLeft, SpriteEffects.None, 0f);
            sb.Draw(texture, size with { Y = size.Bottom, Width = 2, Height = 60 }, null, color, 0f, Origin.BottomLeft, SpriteEffects.None, 0f);

            sb.Draw(texture, size with { X = size.Right, Y = size.Bottom, Width = 60, Height = 2 }, null, color, 0f, Origin.BottomRight, SpriteEffects.None, 0f);
            sb.Draw(texture, size with { X = size.Right, Y = size.Bottom, Width = 2, Height = 60 }, null, color, 0f, Origin.BottomRight, SpriteEffects.None, 0f);
        }
    }
}