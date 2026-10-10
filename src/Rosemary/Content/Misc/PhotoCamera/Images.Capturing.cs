using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReLogic.Content;
using Rosemary.Core;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace Rosemary.Content.Misc;

public static partial class Images
{
    public const int BASE_RESOLUTION = 256;

    private static int ScaledResolution => (int)(BASE_RESOLUTION * Math.Clamp(Main.GameZoomTarget, 1f, 2f));
    
    private static bool captureRequested;
    private static float captureVfx;

    private static string identifierToCreate = string.Empty;

    private static readonly List<IPolaroidCameraFilter> base_filters = [ new BaseFilter() ];

    private static List<IPolaroidCameraFilter> filters = [];

    public static string Capture(PolaroidCamera item)
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
    }

    [ModSystemHooks.PostUpdateEverything]
    private static void PostUpdate_Capturing()
    {
        captureVfx = MathF.Max(0, captureVfx - 0.1f);
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

        // TODO: Account for world edges
        var size = new Vector2(ScaledResolution) * 0.5f;
        var position = Vector2.Clamp(Main.MouseScreen, size, new Vector2(Main.screenWidth, Main.screenHeight) - size) - size;
        var frame = new Rectangle((int)position.X, (int)position.Y, ScaledResolution, ScaledResolution);

        DrawCameraOverlay(sb, position);

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
                sb.Draw(Main.finalScreenTarget, Main.graphics.GraphicsDevice.Viewport.Bounds, frame, Color.White);
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
    
    private static void DrawCameraOverlay(SpriteBatch sb, Vector2 position)
    {
        var alive = Main.LocalPlayer is { noItems: false, CCed: false, dead: false };

        var texture = TextureAssets.MagicPixel.Value;
        var frame = new Rectangle((int)position.X, (int)position.Y, ScaledResolution, ScaledResolution);

        if (!alive
         || Main.LocalPlayer.HeldItem.type != ModContent.ItemType<PolaroidCamera>()
         || Main.LocalPlayer.lastMouseInterface)
        {
            return;
        }

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        {
            DrawFrame();

            sb.Draw(texture, frame, Color.White * captureVfx);
        }
        sb.End();

        return;

        void DrawFrame()
        {
            sb.Draw(texture, frame with { Width = 2 }, Color.White);
            sb.Draw(texture, frame with { X = frame.Right - 2, Width = 2 }, Color.White);

            sb.Draw(texture, frame with { Height = 2 }, Color.White);
            sb.Draw(texture, frame with { Y = frame.Bottom - 2, Height = 2 }, Color.White);

            var size = (int)(ScaledResolution * 0.5f) - 30;

            sb.Draw(texture, frame with { Width = 2, X = frame.Center.X, Height = size }, Color.White);
            sb.Draw(texture, frame with { Width = 2, X = frame.Center.X, Height = size, Y = frame.Bottom - size}, Color.White);

            sb.Draw(texture, frame with { Height = 2, Y = frame.Center.Y, Width = size }, Color.White);
            sb.Draw(texture, frame with { Height = 2, Y = frame.Center.Y, Width = size, X = frame.Right - size }, Color.White);
        }
    }
}