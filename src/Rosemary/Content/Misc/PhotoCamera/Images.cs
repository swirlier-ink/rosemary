using Microsoft.Xna.Framework.Graphics;
using Rosemary.Common.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Terraria;
using Terraria.ID;

namespace Rosemary.Content.Misc;

public static partial class Images
{
    private const string IMAGE_EXTENSION = ".jpg";

    public static string ImageSavesPath => Path.Combine(RosemaryIO.SavePath, "polaroids");

    private static readonly Dictionary<string, Texture2D> image_cache = [];

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
        var path = Path.Combine(ImageSavesPath, Path.ChangeExtension(id, IMAGE_EXTENSION));

        return path;
    }

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

        if (local_identifiers.Contains(id) && Directory.Exists(ImageSavesPath))
        {
            var path = GetImagePath(id);

            using var stream = File.OpenRead(path);

            image = Texture2D.FromStream(Main.graphics.GraphicsDevice, stream);

            image_cache.Add(id, image);

            return true;
        }

        if (Main.netMode == NetmodeID.MultiplayerClient
         && multiplayer_host_identifiers.Contains(id)
         && waiting_on_identifiers.Add(id))
        {
            new ImageFromHostPacket(Main.myPlayer, true, id).Send();
        }

        return false;
    }
}
