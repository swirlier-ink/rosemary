using MonoMod.Cil;
using System.Reflection;
using Rosemary.Common;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Rosemary.Content.Misc;

public static partial class MiscItemSets
{
    [OnLoad]
    private static void Load_HasRightClickSound()
    {
        MonoModHooks.Modify(
            typeof(ItemLoader).GetMethod(
                nameof(ItemLoader.RightClick),
                BindingFlags.Public | BindingFlags.Static
            ),
            RightClick_HasRightClickSound
        );
        IL_ItemSlot.RightClick += _ => { };
        IL_ItemSlot.GetGamepadInstructions_ItemArray_int_int += _ => { };
    }

    private static void RightClick_HasRightClickSound(ILContext il)
    {
        var c = new ILCursor(il);

        var itemIndex = ParameterIndex.Invalid;

        var jumpPlaySoundLabel = c.DefineLabel();

        c.GotoNext(
            i => i.MatchLdarg(out itemIndex),
            i => i.MatchLdarg(out int _),
            i => i.MatchCall(typeof(ItemLoader), nameof(ItemLoader.RightClickCallHooks))
        );

        c.GotoNext(
            MoveType.Before,
            i => i.MatchLdcI4(7)
        );

        c.MoveAfterLabels();

        c.EmitLdarg(itemIndex);
        c.EmitDelegate(
            static (Item item) =>
            {
                return ItemID.Sets.HasRightClickSound[item.type];
            }
        );
        c.EmitBrtrue(jumpPlaySoundLabel);

        c.GotoNext(
            MoveType.After,
            i => i.MatchCall(typeof(SoundEngine), nameof(SoundEngine.PlaySound)),
            i => i.MatchPop()
        );

        c.MarkLabel(jumpPlaySoundLabel);
    }
}
