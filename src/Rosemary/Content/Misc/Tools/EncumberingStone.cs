using Terraria;
using Terraria.ModLoader;

namespace Rosemary.Content.Misc;

public sealed class EncumberingStoneToggle : CommonBuilderToggle
{
    public override string Texture => Assets.Misc.EncumberingStone.KEY;

    public override string HoverTexture => Assets.Misc.EncumberingStone.KEY;

    public override int NumberOfStates => 2;

    public override Position OrderPosition => new Before(BlockSwap);

    public override bool Active() => Main.LocalPlayer.UnlockedEncumberingStone;

    [ModPlayerHooks.UpdateEquips]
    private static void UpdateEquips(ModPlayer self)
    {
        self.Player.preventAllItemPickups = ModContent.GetInstance<EncumberingStoneToggle>().CurrentState == 0;
    }

    public override string DisplayValue()
    {
        return
            CurrentState == 0
                ? Mods.Rosemary.Content.Misc.EncumberingStone.On.GetTextValue()
                : Mods.Rosemary.Content.Misc.EncumberingStone.Off.GetTextValue();
    }
}
