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

            ScanItems(chest);
        }

        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            ScanPlayer(Main.LocalPlayer);
        }

        return;

        static void ScanPlayer(Player player)
        {
            ScanItems(player.armor);
            ScanItems(player.dye);

            // Technically overkill as polaroids cant really be in these slots, but
            foreach (var loadout in player.Loadouts)
            {
                ScanItems(loadout.Armor);
                ScanItems(loadout.Dye);
            }

            ScanItems(player.inventory);
            ScanItems(player.miscEquips);
            ScanItems(player.miscDyes);
            ScanItems(player.bank);
            ScanItems(player.bank2);
            ScanItems(player.bank3);
            ScanItems(player.bank4);
        }
    }

    [ModSystemHooks.OnWorldUnload]
    private static void OnWorldUnload_Development()
    {
        polaroids_to_update.Clear();
    }

    private static void ScanItems(Chest chest) => ScanItems(chest.item);

    private static void ScanItems(Item[] items)
    {
        foreach (var item in items)
        {
            if (item.ModItem is not PolaroidItem polaroid)
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