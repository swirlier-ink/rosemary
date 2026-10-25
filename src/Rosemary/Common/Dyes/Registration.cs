using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Common;

// From NIGHTSHADE, with permission

/// <summary>
///     Responsible for registering dyes and managing auto-generation of dyes.
/// </summary>
public static class DyeRegistry
{
    private static readonly IDyeDefinition[] vanilla_dyes =
    [
        BasicColorDye.Red,
        BasicColorDye.Orange,
        BasicColorDye.Yellow,
        BasicColorDye.Lime,
        BasicColorDye.Green,
        BasicColorDye.Teal,
        BasicColorDye.Cyan,
        BasicColorDye.SkyBlue,
        BasicColorDye.Blue,
        BasicColorDye.Purple,
        BasicColorDye.Violet,
        BasicColorDye.Pink,
        BasicColorDye.Brown,
        BrightnessDye.Silver,
        BrightnessDye.Black,
        BrightColorDye.Red,
        BrightColorDye.Orange,
        BrightColorDye.Yellow,
        BrightColorDye.Lime,
        BrightColorDye.Green,
        BrightColorDye.Teal,
        BrightColorDye.Cyan,
        BrightColorDye.SkyBlue,
        BrightColorDye.Blue,
        BrightColorDye.Purple,
        BrightColorDye.Violet,
        BrightColorDye.Pink,
        BrightColorDye.Brown,
        BrightColorDye.Silver,
        GradientDye.Flame,
        GradientDye.GreenFlame,
        GradientDye.BlueFlame,
        GradientDye.YellowGradient,
        GradientDye.CyanGradient,
        GradientDye.VioletGradient,
        GradientDye.Rainbow,
        GradientDye.IntenseFlame,
        GradientDye.IntenseGreenFlame,
        GradientDye.IntenseBlueFlame,
        GradientDye.IntenseRainbow,
        SilverCompoundDye.Red,
        SilverCompoundDye.Orange,
        SilverCompoundDye.Yellow,
        SilverCompoundDye.Lime,
        SilverCompoundDye.Green,
        SilverCompoundDye.Teal,
        SilverCompoundDye.Cyan,
        SilverCompoundDye.SkyBlue,
        SilverCompoundDye.Blue,
        SilverCompoundDye.Purple,
        SilverCompoundDye.Violet,
        SilverCompoundDye.Pink,
        SilverCompoundDye.Brown,
        BlackCompoundDye.Red,
        BlackCompoundDye.Orange,
        BlackCompoundDye.Yellow,
        BlackCompoundDye.Lime,
        BlackCompoundDye.Green,
        BlackCompoundDye.Teal,
        BlackCompoundDye.Cyan,
        BlackCompoundDye.SkyBlue,
        BlackCompoundDye.Blue,
        BlackCompoundDye.Purple,
        BlackCompoundDye.Violet,
        BlackCompoundDye.Pink,
        BlackCompoundDye.Brown,
        BlackCompoundDye.Silver,
        GradientCompoundDye.FlameAndBlack,
        GradientCompoundDye.GreenFlameAndBlack,
        GradientCompoundDye.BlueFlameAndBlack,
        GradientCompoundDye.FlameAndSilver,
        GradientCompoundDye.GreenFlameAndSilver,
        GradientCompoundDye.BlueFlameAndSilver,
        SpecialDye.BlackAndWhite,
        SpecialDye.Acid,
        SpecialDye.BlueAcid,
        SpecialDye.RedAcid,
        SpecialDye.Chlorophyte,
        SpecialDye.Gel,
        SpecialDye.GlowingMushroom,
        SpecialDye.Grim,
        SpecialDye.Hades,
        SpecialDye.BurningHades,
        SpecialDye.ShadowflameHades,
        SpecialDye.LivingOcean,
        SpecialDye.LivingFlame,
        SpecialDye.LivingRainbow,
        SpecialDye.Martian,
        SpecialDye.MidnightRainbow,
        SpecialDye.Mirage,
        SpecialDye.Negative,
        SpecialDye.Pixie,
        SpecialDye.Phase,
        SpecialDye.PurpleOoze,
        SpecialDye.Reflective,
        SpecialDye.ReflectiveCopper,
        SpecialDye.ReflectiveGold,
        SpecialDye.ReflectiveObsidian,
        SpecialDye.ReflectiveMetal,
        SpecialDye.ReflectiveSilver,
        SpecialDye.Shadow,
        SpecialDye.ShiftingSands,
        SpecialDye.SkiphsBlood,
        SpecialDye.Twilight,
        SpecialDye.Wisp,
        SpecialDye.InfernalWisp,
        SpecialDye.UnicornWisp,
        SpecialDye.PinkGel,
        SpecialDye.ShiftingPearlSands,
        SpecialDye.Nebula,
        SpecialDye.Solar,
        SpecialDye.Stardust,
        SpecialDye.Vortex,
        SpecialDye.Void,
        SpecialDye.Lokis,
        SpecialDye.Team,
        SpecialDye.BloodBath,
        SpecialDye.Fogbound,
        SpecialDye.Prismatic,
        SpecialDye.ColorOnly,
    ];

    private static readonly IDyeDefinition[] basic_color_dyes =
    [
        BasicColorDye.Red,
        BasicColorDye.Orange,
        BasicColorDye.Yellow,
        BasicColorDye.Lime,
        BasicColorDye.Green,
        BasicColorDye.Teal,
        BasicColorDye.Cyan,
        BasicColorDye.SkyBlue,
        BasicColorDye.Blue,
        BasicColorDye.Purple,
        BasicColorDye.Violet,
        BasicColorDye.Pink,
        BasicColorDye.Brown,
    ];

    private static readonly IDyeDefinition[] basic_dyes_with_brightness =
    [
        ..basic_color_dyes,
        BrightnessDye.Black,
        BrightnessDye.Silver,
    ];

    private static IDyeDefinition?[] dyeDefinitions = [];

    private static Mod Mod => ModContent.GetInstance<ModImpl>();

    [ModSystemHooks.ResizeArrays]
    private static void ResizeArrays()
    {
        dyeDefinitions = CreateSet<IDyeDefinition?>(nameof(dyeDefinitions), null);

        foreach (var dye in vanilla_dyes)
        {
            ItemID.Sets.DyeDefinitions[dye.ItemId] = dye;
        }

        return;

        static T[] CreateSet<T>(string name, T defaultState)
        {
            return ItemID.Sets.Factory.CreateNamedSet(Mod, name)
                         .RegisterCustomSet(defaultState);
        }
    }

    extension(ItemID.Sets)
    {
        public static IDyeDefinition?[] DyeDefinitions => dyeDefinitions;
    }
}
