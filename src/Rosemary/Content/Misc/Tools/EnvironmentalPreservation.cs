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

    public override int NumberOfStates => 2;

    public override bool Active() => Main.LocalPlayer.UnlockedEnvironmentalPreservation;

    [ModPlayerHooks.UpdateEquips]
    private static void UpdateEquips(ModPlayer self)
    {
        self.Player.dontHurtNature = ModContent.GetInstance<EnvironmentalPreservationToggle>().CurrentState == 0;
    }

    public override string DisplayValue()
    {
        return
            CurrentState == 0
                ? Mods.Rosemary.Content.Misc.EnvironmentalPreservation.On.GetTextValue()
                : Mods.Rosemary.Content.Misc.EnvironmentalPreservation.Off.GetTextValue();
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
