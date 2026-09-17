using System;
using Rosemary.Common.IO;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Rosemary.Content.Elk;

public static class UnnamedItems
{
    public static string NamedItemsPath => Path.Combine(RosemaryIO.SavePath, "item_names.rsmry");

    private record struct NameInfo(LocalizedText OriginalName, bool NameCreated);

    private static NameInfo?[] nameInfo = [];

    private static Mod Mod => ModContent.GetInstance<ModImpl>();

    [ModSystemHooks.ResizeArrays]
    private static void ResizeArrays()
    {
        nameInfo = CreateSet<NameInfo?>(nameof(nameInfo), null);

        return;

        static T[] CreateSet<T>(string name, T defaultState)
        {
            return ItemID.Sets.Factory.CreateNamedSet(Mod, name)
                         .RegisterCustomSet(defaultState);
        }
    }

    /// <summary>
    /// Marks the item as "unnamed," should be run once in <see cref="ModItem.SetStaticDefaults"/> or earlier during loading.
    /// </summary>
    /// <param name="type"></param>
    public static void Add(int type)
    {
        nameInfo[type] = new NameInfo(Lang._itemNameCache[type], false);
        Lang._itemNameCache[type] = LocalizedText.Empty;
    }

    public static void Name(int type)
    {
        if (nameInfo[type] is not { } info)
        {
            return;
        }

        info.NameCreated = true;
        Lang._itemNameCache[type] = info.OriginalName;
    }

    [ModSystemHooks.PostSetupContent]
    private static void PostSetupContent() => Load();

    [ModSystemHooks.OnWorldUnload]
    private static void OnWorldUnload() => Save();

    private static void Load()
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

        var namedItems = tag.Get<string[]>(nameof(nameInfo));

        foreach (var name in namedItems)
        {
            if (ModContent.TryFind<ModItem>(name, out var item))
            {
                Name(item.Type);

                continue;
            }

            var id = name.Split('/')[1];

            Name(int.Parse(id));
        }

        return;

        static void CreateEmpty()
        {
            var tag = new TagCompound();

            TagIO.ToFile(tag, NamedItemsPath);
        }
    }

    private static void Save()
    {
        var tag = new TagCompound();

        var namedItems = new List<string>();

        for (var i = 0; i < nameInfo.Length; i++)
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

        tag[nameof(nameInfo)] = namedItems.ToArray();

        TagIO.ToFile(tag, NamedItemsPath);
    }
}
