using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Rosemary.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Content.Misc;

public sealed class DefectiveCameraFilter : ModItem, IPolaroidCameraFilter
{

    
    public override string Texture => Assets.Misc.PolaroidCameraFilters.BlurFilter.KEY;

    public override string LocalizationCategory => "Content.Misc.CameraFilters";

    bool IPolaroidCameraFilter.ApplyFilter(SpriteBatch sb, GraphicsDevice device, RenderTarget2D target, RenderTarget2D swap)
    {
        device.SetRenderTarget(swap);
        device.Clear(Color.Transparent);


        var photoPos = (Images.GetCaptureArea().Center.ToVector2() + Main.screenPosition).ToTileCoordinates();
        
        var ambient = Lighting.GetColor(photoPos).ToVector4();
        #if DEBUG
                Main.NewText(Lighting.GetColor(photoPos).DebugDisplayString);
        #endif
        
        
        var shader = Assets.Misc.PolaroidCameraFilters.DefectivePolaroidShader.CreateDefectivePolaroidShader();

        shader.Parameters.TintPenis = Color.White.ToVector4();
        shader.Parameters.TemperatureApplyAmount = 0.01f;
        
        
        // rudimentary tinting to simulate temperature fucking
        if (Main.LocalPlayer.ZoneDesert || Main.rand.NextBool(7))
        {
            shader.Parameters.TintPenis = new Vector4(246, 112, 138, 255) / 255f;
            shader.Parameters.TemperatureApplyAmount = Main.rand.NextFloat(0.3f, 0.5f);
        }
        if (Main.LocalPlayer.ZoneSnow || Main.rand.NextBool(7))
        {
            shader.Parameters.TintPenis = Color.LightSeaGreen.ToVector4();
            shader.Parameters.TemperatureApplyAmount = Main.rand.NextFloat(0.3f, 0.5f);
        
        }

        //creating underexposed swathes
        
        shader.Parameters.UnderexposurePos = Main.rand.NextVector2Square(0f, 1f);
        shader.Parameters.UnderexposurePow = Main.rand.NextVector2Square(-4f, 4f) * MathF.Pow(Main.rand.NextFloat(0.2f, 0.9f), 3f);
        
        shader.Parameters.AmbientLight = (ambient * 0.1f);
        
        
        shader.Apply();

        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, shader.Shader);
        {
            sb.Draw(target, Vector2.Zero, Color.White);

            
            // drawing webbing

            
        }
        sb.End();

        
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null);
        {
            // drawing webbing
            var webbing = AssetReferences.Assets.Misc.PolaroidCameraFilters.DefectWebs.Asset.Value;
            
            for (int i = 0; i < Main.rand.Next(0, 3); i++)
            {
                var pos = GetRandomPositionAtEdgeOfRect();

                var frame = new Rectangle(0, 64 * Main.rand.Next(0, 4), 64, 64);
                
                // oh yeah rider nice formatting hell yea
                sb.Draw(
                    webbing,
                    pos * target.Bounds.Size(),
                    frame,
                    Color.White * 0.98f,
                    pos.DirectionTo(
                            Main.rand.NextVector2Square(
                                0.4f,
                                0.6f
                            )
                        )
                       .ToRotation(),
                    frame.Size()
                  * new Vector2(
                        0f,
                        0.5f
                    ),
                    new Vector2(1f, 1f * Main.rand.NextFloat(0.8f, 2f)) * Main.rand.NextFloat(
                        0.2f,
                        0.4f
                    ),
                    SpriteEffects.None,
                    1f
                );
            }


            // specialized for this method sorry
            Vector2 GetRandomPositionAtEdgeOfRect()
            {
                var vert = Main.rand.NextBool();
                var pos = new Vector2(0, 0);
                if (vert)
                {
                    pos.X = Main.rand.NextBool() ? 1f : 0.0f;
                    pos.Y = Main.rand.NextFloat();
                }
                else
                {
                    pos.Y = Main.rand.NextBool() ? 1f : 0f;
                    pos.X = Main.rand.NextFloat();
                }

                return pos;
            }
            
        }
        

        sb.End();

        return true;
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
