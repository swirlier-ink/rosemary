using System.ComponentModel.Design;
using System.IO;
using Terraria;

namespace Rosemary.Common.IO;

public static class RosemaryIO
{
    public static string SavePath => Path.Combine(Main.SavePath, "rosemary");
}
