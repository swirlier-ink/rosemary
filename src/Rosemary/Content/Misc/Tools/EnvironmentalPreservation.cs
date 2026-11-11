using System.Linq;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Content.Misc;

public sealed class EnvironmentalPreservationToggle : CommonBuilderToggle
{
    public override string Texture => Assets.Misc.EnvironmentalPreservation.KEY;

    public override Position OrderPosition => new After(BlockSwap);

    public override int NumberOfStates => 2;

    public override bool Active() => Main.LocalPlayer.UnlockedEnvironmentalPreservation;

    [ModPlayerHooks.UpdateEquips]
    private static void UpdateEquips(ModPlayer self)
    {
        self.Player.dontHurtNature = EnvironmentalPreservationToggle.IsActive && EnvironmentalPreservationToggle.State == 0;
    }

    public override string DisplayValue()
    {
        return
            CurrentState == 0
                ? Mods.Rosemary.Content.Misc.CritterCompanionship.On.GetTextValue()
                : Mods.Rosemary.Content.Misc.CritterCompanionship.Off.GetTextValue();
    }
}
