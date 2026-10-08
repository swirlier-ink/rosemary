using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace Rosemary.Common.IO;

public static class RosemaryIO
{
    public static string SavePath => Path.Combine(Main.SavePath, "rosemary");

    [OnLoad]
    private static void Load()
    {
        try
        {
            Directory.CreateDirectory(SavePath);
        }
        catch
        {
            ModContent.GetInstance<ModImpl>().Logger.Warn($"Could not create directory at: \"{SavePath}\"!");
        }
    }
}
