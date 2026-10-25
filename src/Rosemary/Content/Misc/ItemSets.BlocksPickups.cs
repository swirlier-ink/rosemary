using Terraria;
using Terraria.ID;

namespace Rosemary.Content.Misc;

public static partial class MiscItemSets
{
    [OnLoad]
    private static void Load_BlocksItemPickupsWhenHeld()
    {
        On_Player.GrabItems += GrabItems_BlocksItemPickupsWhenHeld;
    }

    private static void GrabItems_BlocksItemPickupsWhenHeld(On_Player.orig_GrabItems orig, Player self, int i)
    {
        if (ItemID.Sets.BlocksItemPickupsWhenHeld[self.HeldItem.type])
        {
            return;
        }

        orig(self, i);
    }
}
