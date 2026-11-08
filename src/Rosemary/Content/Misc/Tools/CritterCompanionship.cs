using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Content.Misc;

public sealed class CritterCompanionshipToggle : BuilderToggle
{
    public override string Texture => Assets.Misc.CritterCompanionship.KEY;

    public override string HoverTexture => Assets.Misc.CritterCompanionship.KEY;

    public override int NumberOfStates => 2;

    public override bool Active() => Main.LocalPlayer.UnlockedCritterCompanionship;

    [ModPlayerHooks.UpdateEquips]
    private static void UpdateEquips(ModPlayer self)
    {
        self.Player.dontHurtCritters = ModContent.GetInstance<CritterCompanionshipToggle>().CurrentState == 0;
    }

    // BuilderToggle.Position is not as powerful as I'd like
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

    public override bool Draw(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams)
    {
        drawParams.Color = Color.White;
        drawParams.Frame = drawParams.Texture.Frame(2, 2, CurrentState);

        return true;
    }

    public override bool DrawHover(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams)
    {
        drawParams.Frame = drawParams.Texture.Frame(2, 2, CurrentState, 1);
        drawParams.Scale = 0.9f;

        return true;
    }

    public override bool OnLeftClick(ref SoundStyle? sound)
    {
        sound = SoundID.Unlock;

        return true;
    }
}
