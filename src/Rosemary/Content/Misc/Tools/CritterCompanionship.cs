using Terraria;
using Terraria.ModLoader;

namespace Rosemary.Content.Misc;

public sealed class CritterCompanionshipToggle : CommonBuilderToggle
{
    public override string Texture => Assets.Misc.CritterCompanionship.KEY;

    public override int NumberOfStates => 2;

    public override bool Active() => Main.LocalPlayer.UnlockedCritterCompanionship;

    [ModPlayerHooks.UpdateEquips]
    private static void UpdateEquips(ModPlayer self)
    {
        self.Player.dontHurtCritters = CritterCompanionshipToggle.IsActive && CritterCompanionshipToggle.State == 0;
    }

    // BuilderToggle.OrderPosition is not as powerful as I'd like
    [ModSystemHooks.ResizeArrays]
    private static void ResizeArrays()
    {
        var toggle = BuilderToggleLoader._drawOrder.Find(t => t is CritterCompanionshipToggle);

        if (toggle is null)
        {
            return;
        }

        BuilderToggleLoader._drawOrder.Remove(toggle);

        var index = BuilderToggleLoader._drawOrder.FindIndex(t => t is EnvironmentalPreservationToggle);

        if (index == -1)
        {
            return;
        }

        BuilderToggleLoader._drawOrder.Insert(index + 1, toggle);
    }

    public override string DisplayValue()
    {
        return
            CurrentState == 0
                ? Mods.Rosemary.Content.Misc.CritterCompanionship.On.GetTextValue()
                : Mods.Rosemary.Content.Misc.CritterCompanionship.Off.GetTextValue();
    }
}
