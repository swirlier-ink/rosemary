using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Content.Misc;

public sealed class EnvironmentalPreservationToggle : BuilderToggle
{
    public override string Texture => Assets.Misc.EnvironmentalPreservation.KEY;

    public override string HoverTexture => Assets.Misc.EnvironmentalPreservation.KEY;

    public override Position OrderPosition => new After(BlockSwap);

    public override int NumberOfStates => 3;

    public override bool Active() => Main.LocalPlayer.UnlockedEnvironmentalPreservation;

    public override void Load()
    {
        On_Player.GetTileCutIgnorance += GetTileCutIgnorance_WholeOverride;
    }

    // ReSharper disable once InconsistentNaming
    private static bool[] all_true = [];

    [ModSystemHooks.ResizeArrays]
    private static void ResizeArrays()
    {
        all_true = Enumerable.Repeat(true, TileLoader.TileCount).ToArray();
    }

    // Notably, this only runs on the client, so checking builder toggles like such should be fine.
    private static bool[] GetTileCutIgnorance_WholeOverride(On_Player.orig_GetTileCutIgnorance orig, Player self, bool allowRegrowth, bool fromTrap)
    {
        var ret = orig(self, allowRegrowth, fromTrap);

        if (fromTrap
         || !self.dontHurtNature
         || ModContent.GetInstance<EnvironmentalPreservationToggle>().CurrentState != 1)
        {
            return ret;
        }

        return all_true;
    }

    [ModPlayerHooks.UpdateEquips]
    private static void UpdateEquips(ModPlayer self)
    {
        self.Player.dontHurtNature = ModContent.GetInstance<EnvironmentalPreservationToggle>().CurrentState != 2;
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

    public override bool Draw(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams)
    {
        drawParams.Color = Color.White;
        drawParams.Frame = drawParams.Texture.Frame(NumberOfStates, 2, CurrentState);

        return true;
    }

    public override bool DrawHover(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams)
    {
        drawParams.Frame = drawParams.Texture.Frame(NumberOfStates, 2, CurrentState, 1);
        drawParams.Scale = 0.9f;

        return true;
    }

    public override bool OnLeftClick(ref SoundStyle? sound)
    {
        sound = SoundID.Unlock;

        return true;
    }
}
