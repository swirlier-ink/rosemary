using Rosemary.Common.IO;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Rosemary.Content.Elk;

// Should be noted that this will NOT work with elklang named items, as they've already been "named."
public static partial class ElkLangItemSets
{
    public static string NamedItemsPath => Path.Combine(RosemaryIO.SavePath, "item_names.rsmry");

    private static readonly Dictionary<int, LocalizedText> unnamed_prior_names = [];

    [OnLoad]
    private static void Load_Unnamed()
    {

    }

    public static void Name(int type)
    {
        if (!unnamed[type])
        {
            return;
        }

        unnamed[type] = false;
        Lang._itemNameCache[type] = unnamed_prior_names[type];
    }

    [ModSystemHooks.PostSetupContent]
    private static void PostSetupContent() => LoadNames();

    [ModPlayerHooks.PostSavePlayer]
    private static void PostSavePlayer() => SaveNames();

    [ModSystemHooks.OnWorldUnload]
    private static void OnWorldUnload() => SaveNames();

    // Ran after ItemLoader.FinishSetup, TODO: Move to a separate hook? Should this load order be relied on?
    [ModSystemHooks.ModifyGameTipVisibility]
    private static void ModifyGameTipVisibility(IReadOnlyList<GameTipData> gameTips)
    {
        for (var i = 0; i < unnamed.Length; i++)
        {
            if (!unnamed[i])
            {
                continue;
            }

            unnamed_prior_names[i] = Lang._itemNameCache[i];
            Lang._itemNameCache[i] = LocalizedText.Empty;
        }
    }

    private static void LoadNames()
    {
        try
        {
            Directory.CreateDirectory(RosemaryIO.SavePath);
        }
        catch
        {
            ModContent.GetInstance<ModImpl>().Logger.Warn($"Could not create directory at: \"{RosemaryIO.SavePath}\"!");
        }

        if (!File.Exists(NamedItemsPath))
        {
            CreateEmpty();

            return;
        }

        var tag = TagIO.FromFile(NamedItemsPath);

        var namedItems = tag.Get<string[]>(nameof(unnamed));

        foreach (var name in namedItems)
        {
            if (ModContent.TryFind<ModItem>(name, out var item))
            {
                MarkNamed(item.Type);

                continue;
            }

            var id = name.Split('/')[1];

            MarkNamed(int.Parse(id));
        }

        return;

        static void CreateEmpty()
        {
            var tag = new TagCompound();

            TagIO.ToFile(tag, NamedItemsPath);
        }

        static void MarkNamed(int type)
        {
            unnamed[type] = false;
        }
    }

    private static void SaveNames()
    {
        var tag = new TagCompound();

        var namedItems = new List<string>();

        for (var i = 0; i < unnamed.Length; i++)
        {
            if (ItemLoader.GetItem(i) is { } modItem)
            {
                namedItems.Add(modItem.FullName);
            }
            else
            {
                namedItems.Add($"Terraria/{i}");
            }
        }

        tag[nameof(unnamed)] = namedItems.ToArray();

        TagIO.ToFile(tag, NamedItemsPath);
    }

#if DEBUG
    public sealed class ResetNamesCommand : ModCommand
    {
        public override string Command => "clrnames";

        public override CommandType Type => CommandType.Chat;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var tag = new TagCompound();

            TagIO.ToFile(tag, NamedItemsPath);
        }
    }
#endif
}
