using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace Rosemary.Content.Misc;

public static class ImageCapturing
{
    public static string ImageSavesPath => Path.Combine(RosemaryIO.SavePath, "polaroids");
    private static int ScaledResolution => (int)(resolution * Math.Clamp(Main.GameZoomTarget, 1f, 2f));
    
    private const int resolution = 256;

    private static bool captureRequested;
    private static float captureVfx;

    public static void Capture()
    {
        captureVfx = 1f;
        captureRequested = true;
    }

    private static string GenerateImageId()
    {
        var id = "";

        var playerName = Regex.Replace(Main.LocalPlayer.name, @"[<>:""/\\|?* ]", "").ToUpper();
        var date = DateTime.Now.ToString("ddMMyyHHmmssfff");
        var unique = Main.rand.Next(255).ToString("X");
        
        id = playerName + date + unique;

        if (string.IsNullOrWhiteSpace(id))
            id = "thisshouldprobablybenamedsomethingelse" + Main.rand.Next(255).ToString("X");
        
        return id;
    }

    [OnLoad]
    private static void Load()
    {
        On_FilterManager.EndCapture_RenderTarget2D_RenderTarget2D_RenderTarget2D_Vector2_Vector2_Vector2 += HandleCapture;
    }

    private static void HandleCapture(On_FilterManager.orig_EndCapture_RenderTarget2D_RenderTarget2D_RenderTarget2D_Vector2_Vector2_Vector2 orig, FilterManager self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Vector2 screenSize, Vector2 sceneSize, Vector2 sceneOffset)
    {
        orig(self, finalTexture, screenTarget1, screenTarget2, screenSize, sceneSize, sceneOffset);
        
        // TODO: account for world edges
        var position = ScaledMousePosition() - new Vector2(ScaledResolution) / 2f;
        var frame = new Rectangle((int)position.X, (int)position.Y, ScaledResolution, ScaledResolution);

        DrawCameraOverlay(position);

        captureVfx = MathF.Max(0, captureVfx - 0.1f);
        
        if (Main.gameMenu || !captureRequested) 
            return;
        
        using var lease = ScreenspaceTargetProvider.Shared.Create(Main.graphics.GraphicsDevice, (_, _, targetWidth, targetHeight) => (ScaledResolution, ScaledResolution));
        
        using (lease.Scope(clearColor: Color.Transparent))
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            {
                Main.spriteBatch.Draw(Main.finalScreenTarget, Vector2.Zero, frame, Color.White);
            }
            Main.spriteBatch.End();
        }

        var id = GenerateImageId();
        const string format = ".jpg";
        
        using (var stream = new FileStream(Path.Combine(ImageSavesPath, id + format), FileMode.Create)) 
        {
            lease.Target.SaveAsJpeg(stream, ScaledResolution, ScaledResolution);
        }

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
    
    private static void DrawCameraOverlay(Vector2 position)
    {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        {
            var texture = TextureAssets.MagicPixel.Value;
            var frame = new Rectangle((int)position.X, (int)position.Y, ScaledResolution, ScaledResolution);

            if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<PolaroidCamera>())
                Main.spriteBatch.Draw(texture, frame, Color.White * 0.1f);
            
            Main.spriteBatch.Draw(texture, frame, Color.White * captureVfx);
        }
        Main.spriteBatch.End();
    }
}