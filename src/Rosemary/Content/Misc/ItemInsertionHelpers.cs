using System;
using Terraria;

namespace Rosemary.Content.Misc;

public static class ItemInsertion
{
    public static void HandleRightClick(Item item, Player player, Func<Item, bool> canInsert, Action<Item, Item> insertAction, Action removeAction)
    {
        item.stack++;

        if (Main.mouseItem.IsAir)
        {
            removeAction();

            return;
        }

        if (!canInsert(Main.mouseItem))
        {
            return;
        }

        var mouseItem = Main.mouseItem.Clone();
        mouseItem.stack = 1;

        Main.mouseItem.stack--;

        if (item.stack > 2)
        {
            item.stack--;

            var newCamera = item.Clone();
            {
                newCamera.stack = 1;

                insertAction(newCamera, mouseItem);
            }
            if (Main.mouseItem.stack > 0)
            {
                player.QuickSpawnItem(player.GetItemSource_Item(item), newCamera);
            }
            else
            {
                Main.mouseItem = newCamera;
            }
        }
        else
        {
            insertAction(item, mouseItem);
        }
    }
}
