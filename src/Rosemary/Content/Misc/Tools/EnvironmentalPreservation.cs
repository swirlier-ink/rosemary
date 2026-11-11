using System.Linq;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Content.Misc;

public sealed class EnvironmentalPreservationToggle : CommonBuilderToggle
{
    public override string Texture => Assets.Misc.EnvironmentalPreservation.KEY;

    public override string HoverTexture => Assets.Misc.EnvironmentalPreservation.KEY;

    public override Position OrderPosition => new After(BlockSwap);

    public override int NumberOfStates => 3;

    public override bool Active() => Main.LocalPlayer.UnlockedEnvironmentalPreservation;

    public override void Load()
    {
        // Notably, these only run on the client, so checking builder toggles directly should be fine.
        On_Player.GetTileCutIgnorance += GetTileCutIgnorance_WholeOverride;
        IL_Player.ApplyTouchDamage += ApplyTouchDamage_DisableKillTile;
    }

    // ReSharper disable once InconsistentNaming
    private static bool[] all_true = [];

    [ModSystemHooks.ResizeArrays]
    private static void ResizeArrays()
    {
        all_true = Enumerable.Repeat(true, TileLoader.TileCount).ToArray();
    }

    private static void ApplyTouchDamage_DisableKillTile(ILContext il)
    {
        var c = new ILCursor(il);

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdsfld(typeof(TileID.Sets), nameof(TileID.Sets.TouchDamageDestroyTile)),
            i => i.MatchLdarg(out int _),
            i => i.MatchLdelemU1()
        );

        c.EmitDelegate(static () => !EnvironmentalPreservationToggle.IsActive || EnvironmentalPreservationToggle.State != 1);
        c.EmitAnd();
    }

    private static bool[] GetTileCutIgnorance_WholeOverride(On_Player.orig_GetTileCutIgnorance orig, Player self, bool allowRegrowth, bool fromTrap)
    {
        var ret = orig(self, allowRegrowth, fromTrap);

        if (fromTrap
         || !self.dontHurtNature
         || EnvironmentalPreservationToggle.State != 1)
        {
            return ret;
        }

        return all_true;
    }

    [ModPlayerHooks.UpdateEquips]
    private static void UpdateEquips(ModPlayer self)
    {
        self.Player.dontHurtNature = EnvironmentalPreservationToggle.IsActive && EnvironmentalPreservationToggle.State != 2;
    }

    public override string DisplayValue()
    {
        return CurrentState switch
        {
            0 => Mods.Rosemary.Content.Misc.EnvironmentalPreservation.On.GetTextValue(),
            1 => Mods.Rosemary.Content.Misc.EnvironmentalPreservation.High.GetTextValue(),
            _ => Mods.Rosemary.Content.Misc.EnvironmentalPreservation.Off.GetTextValue(),
        };
    }
}
