using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace Rosemary.Content.Misc;

public static partial class Images
{
    private const string image_extension = ".jpg";

    public static string ImageSavesPath => Path.Combine(RosemaryIO.SavePath, "polaroids");

    private static readonly Dictionary<string, Texture2D> image_cache = [];

    private static Mod Mod => ModContent.GetInstance<ModImpl>();

    [OnUnload]
    private static void Unload()
    {
        foreach (var pair in image_cache)
        {
            pair.Value.Dispose();
            image_cache.Remove(pair.Key);
        }
    }

    private static string CreateIdentifier()
    {
        var playerName = string.Join(string.Empty, Main.LocalPlayer.name.Split(Path.GetInvalidFileNameChars())).ToUpper();

        var date = DateTime.Now.ToString("ddMMyyHHmmssfff");
        var unique = Main.rand.Next(255).ToString("X");

        var id = playerName + date + unique;

        return id;
    }

    private static string GetImagePath(string id)
    {
        var path = Path.Combine(ImageSavesPath, Path.ChangeExtension(id, image_extension));

        return path;
    }

    /// <summary>
    ///     Attempts to grab the image with identifier <paramref name="id"/>
    ///     from either local files, or request it from the host (server) in multiplayer.
    /// </summary>
    /// <returns>
    ///     <see langword="true"/> if the image was obtained successfully.
    /// </returns>
    public static bool TryRequestImage(string id, [NotNullWhen(true)] out Texture2D? image)
    {
        image = null;

        if (Main.dedServ)
        {
            return false;
        }

        if (image_cache.TryGetValue(id, out image))
        {
            return true;
        }

        if (local_identifiers.Contains(id))
        {
            if (LoadLocalImage(out image))
            {
                return true;
            }

            // If the file doesn't exist, or otherwise shouldn't be
            // loaded (e.g. egregious file size,) remove it from the local identifiers
            local_identifiers.Remove(id);
        }

        if (Main.netMode == NetmodeID.MultiplayerClient
         && multiplayer_host_identifiers.Contains(id)
         && waiting_on_identifiers.Add(id))
        {
            new RequestImageFromHostPacket(Main.myPlayer, id).Send(PacketDestination.Broadcast);
        }

        return false;

        unsafe bool LoadLocalImage([NotNullWhen(true)] out Texture2D? image)
        {
            image = null;

            var path = GetImagePath(id);
            
            if (!File.Exists(path))
            {
                return false;
            }

            var info = new FileInfo(path);

            // 1mb limit, overkill?
            if (info.Length >= 1024 * 1024)
            {
                return false;
            }

            try
            {
                using var stream = File.OpenRead(path);

                // TODO: Figure out why this silent crashes, and see if SixLabors.ImageSharp is required
                // By all means this *should* work, but for some esoteric reason beyond human comprehension it doesn't
                // image = Texture2D.FromStream(Graphics.Device, stream, BASE_RESOLUTION, BASE_RESOLUTION, false);

                var sharpImage = Image.Load<Rgba32>(stream);

                if (sharpImage.Width != BASE_RESOLUTION
                 || sharpImage.Height != BASE_RESOLUTION)
                {
                    return false;
                }

                var colorBuffer = new XnaColor[sharpImage.Width * sharpImage.Height];

                for (var i = 0; i < sharpImage.Width; i++)
                {
                    for (var j = 0; j < sharpImage.Height; j++)
                    {
                        var col = sharpImage[i, j];

                        colorBuffer[i + j * sharpImage.Width] = new XnaColor(col.R, col.G, col.B, col.A);
                    }
                }

                image = new Texture2D(Graphics.Device, sharpImage.Width, sharpImage.Height);

                fixed (XnaColor* pColorBuffer = &colorBuffer[0])
                {
                    image.SetDataPointerEXT(0, null, (nint)pColorBuffer, sharpImage.Width * sharpImage.Height * 4);
                }

                image_cache.Add(id, image);

                return true;
            }
            catch (Exception e)
            {
                Mod.Logger.Warn($"Failed to load polaroid image with identifier \"{id}\": {e}");
            }

            return false;
        }
    }
}
