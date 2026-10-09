using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Rosemary.Content.Misc;

public static partial class Images
{
    extension(PolaroidItem item)
    {
        public uint Lifetime => Lifetimes[item.ImageIdentifier];
    }
    
    public static Dictionary<string, uint> Lifetimes = new();
    
    [ModSystemHooks.PostUpdateEverything]
    private static void UpdatePolaroidLifetimes()
    {
        foreach (var pair in Lifetimes)
        {
            Lifetimes[pair.Key]++;
        }
    }
    
    public static bool IsOld(string identifier) => Lifetimes[identifier] > 60 * 3600 * 4;
    public static float BlackFade(string identifier) => 1f - MathHelper.Clamp((Lifetimes[identifier] - 120f) / 400f, 0, 1);
}