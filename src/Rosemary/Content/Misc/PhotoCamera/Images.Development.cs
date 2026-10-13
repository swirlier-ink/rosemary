using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace Rosemary.Content.Misc;

public static partial class Images
{
    private static readonly HashSet<PolaroidItem> polaroids_to_update = [];

    [ModSystemHooks.PostWorldLoad]
    private static void PostWorldLoad_Development()
    {
        polaroids_to_update.Clear();

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            return;
        }

        foreach (var chest in Main.chest)
        {
            if (chest is null)
            {
                continue;
            }

            ScanItems_Development(chest);
        }

        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            ScanPlayer(Main.LocalPlayer);
        }

        return;

        static void ScanPlayer(Player player)
        {
            ScanItems_Development(player.armor);
            ScanItems_Development(player.dye);

            // Technically overkill as polaroids cant really be in these slots, but
            foreach (var loadout in player.Loadouts)
            {
                ScanItems_Development(loadout.Armor);
                ScanItems_Development(loadout.Dye);
            }

            ScanItems_Development(player.inventory);
            ScanItems_Development(player.miscEquips);
            ScanItems_Development(player.miscDyes);
            ScanItems_Development(player.bank);
            ScanItems_Development(player.bank2);
            ScanItems_Development(player.bank3);
            ScanItems_Development(player.bank4);
        }
    }

    [ModSystemHooks.OnWorldUnload]
    private static void OnWorldUnload_Development()
    {
        polaroids_to_update.Clear();
    }

    private static void ScanItems_Development(Chest chest) => ScanItems_Development(chest.item);

    private static void ScanItems_Development(Item[] items)
    {
        foreach (var item in items)
        {
            if (item.ModItem is not PolaroidItem { Original: true } polaroid)
            {
                continue;
            }

            polaroids_to_update.Add(polaroid);
        }
    }

    public static void TrackImageDevelopment(PolaroidItem item) => polaroids_to_update.Add(item);

    [ModSystemHooks.PostUpdateEverything]
    private static void PostUpdate_Development()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            return;
        }

        foreach (var polaroid in polaroids_to_update)
        {
            if (polaroid.Age < uint.MaxValue)
            {
                polaroid.Age++;
            }
        }
    }
}