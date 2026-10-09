using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace Rosemary.Content.Misc;

public static partial class Images
{
    private static int ScaledResolution => (int)(resolution * Math.Clamp(Main.GameZoomTarget, 1f, 2f));
    
    private const int resolution = 256;

    private static bool captureRequested;
    private static float captureVfx;

    private static string identifierToCreate = string.Empty;

    public static string Capture()
    {
        captureVfx = 1f;
        captureRequested = true;

        identifierToCreate = CreateIdentifier();

        return identifierToCreate;
    }

    [OnLoad(Side = ModSide.Client)]
    private static void Load_Capturing()
    {
        On_FilterManager.EndCapture_RenderTarget2D_RenderTarget2D_RenderTarget2D_Vector2_Vector2_Vector2 += HandleCapture;
    }

    private static void HandleCapture(On_FilterManager.orig_EndCapture_RenderTarget2D_RenderTarget2D_RenderTarget2D_Vector2_Vector2_Vector2 orig, FilterManager self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Vector2 screenSize, Vector2 sceneSize, Vector2 sceneOffset)
    {
        orig(self, finalTexture, screenTarget1, screenTarget2, screenSize, sceneSize, sceneOffset);

        var sb = Main.spriteBatch;

        if (Main.gameMenu
         || !captureRequested
         || string.IsNullOrEmpty(identifierToCreate))
        {
            return;
        }

        // TODO: account for world/screen edges
        var position = ScaledMousePosition() - new Vector2(ScaledResolution) * 0.5f;
        var frame = new Rectangle((int)position.X, (int)position.Y, ScaledResolution, ScaledResolution);

        DrawCameraOverlay(sb, position);

        captureVfx = MathF.Max(0, captureVfx - 0.1f);
        
        using var lease = RenderTargetPool.Shared.Rent(Main.graphics.GraphicsDevice, ScaledResolution, ScaledResolution);
        
        using (lease.Scope(clearColor: Color.Transparent))
        {
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            {
                sb.Draw(Main.finalScreenTarget, Vector2.Zero, frame, Color.White);
            }
            sb.End();
        }

        var id = identifierToCreate;

        using var stream = new FileStream(GetImagePath(id), FileMode.Create);

        lease.Target.SaveAsJpeg(stream, ScaledResolution, ScaledResolution);

        // May differ in quality compared to the saved file?
        image_cache[id] = lease.Target;

        captureRequested = false;

        return;

        static Vector2 ScaledMousePosition()
        {
            PlayerInput.SetZoom_Unscaled();
            var mouseScreen = Main.MouseScreen;
            PlayerInput.SetZoom_MouseInWorld();

            return mouseScreen;
        } 
    }
    
    private static void DrawCameraOverlay(SpriteBatch sb, Vector2 position)
    {
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        {
            var texture = TextureAssets.MagicPixel.Value;
            var frame = new Rectangle((int)position.X, (int)position.Y, ScaledResolution, ScaledResolution);

            if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<PolaroidCamera>())
            {
                sb.Draw(texture, frame, Color.White * 0.1f);
            }

            sb.Draw(texture, frame, Color.White * captureVfx);
        }
        sb.End();
    }
}