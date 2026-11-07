using System;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;

namespace Rosemary.Common;

public readonly ref struct LoadingSubTextScope : IDisposable
{
    private readonly string prior;

    public LoadingSubTextScope(string text)
    {
        prior = Interface.loadMods.subProgress.Text;

        Interface.loadMods.SubProgressText = text;
    }

    public void Dispose()
    {
        Interface.loadMods.SubProgressText = prior;
    }
}

public static class ModLoaderExtensions
{
    extension(ModLoader)
    {
        public static LoadingSubTextScope LoadingSubText(string text) => new LoadingSubTextScope(text);
    }
}
