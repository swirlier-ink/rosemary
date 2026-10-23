using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace Rosemary.Common;

// From NIGHTSHADE, with permission

// This file provides strongly typed references to all vanilla dyes.  It is used
// primarily to aid in the auto-generation of new dyes.

/// <summary>
///     Represents a dye definition.
/// </summary>
public interface IDyeDefinition
{
    /// <summary>
    ///     Whether this dye already exists in vanilla.
    /// </summary>
    bool IsVanilla { get; }

    /// <summary>
    ///     The corresponding item ID.
    /// </summary>
    int ItemId { get; }

    /// <summary>
    ///     The lid type.
    /// </summary>
    DyeLidType LidType { get; }

    /// <summary>
    ///     Any applicable overlays.
    /// </summary>
    DyeOverlayType Overlays { get; }
}

/// <summary>
///     Represents a basic color dye; that is, a base color.
/// </summary>
public sealed class BasicColorDye(int thisId, int blackId, int brightId, int silverId) : IDyeDefinition
{
    public static BasicColorDye Red { get; } = new(ItemID.RedDye, ItemID.RedandBlackDye, ItemID.BrightRedDye, ItemID.RedandSilverDye);

    public static BasicColorDye Orange { get; } = new(ItemID.OrangeDye, ItemID.OrangeandBlackDye, ItemID.BrightOrangeDye, ItemID.OrangeandSilverDye);

    public static BasicColorDye Yellow { get; } = new(ItemID.YellowDye, ItemID.YellowandBlackDye, ItemID.BrightYellowDye, ItemID.YellowandSilverDye);

    public static BasicColorDye Lime { get; } = new(ItemID.LimeDye, ItemID.LimeandBlackDye, ItemID.BrightLimeDye, ItemID.LimeandSilverDye);

    public static BasicColorDye Green { get; } = new(ItemID.GreenDye, ItemID.GreenandBlackDye, ItemID.BrightGreenDye, ItemID.GreenandSilverDye);

    public static BasicColorDye Teal { get; } = new(ItemID.TealDye, ItemID.TealandBlackDye, ItemID.BrightTealDye, ItemID.TealandSilverDye);

    public static BasicColorDye Cyan { get; } = new(ItemID.CyanDye, ItemID.CyanandBlackDye, ItemID.BrightCyanDye, ItemID.CyanandSilverDye);

    public static BasicColorDye SkyBlue { get; } = new(ItemID.SkyBlueDye, ItemID.SkyBlueandBlackDye, ItemID.BrightSkyBlueDye, ItemID.SkyBlueandSilverDye);

    public static BasicColorDye Blue { get; } = new(ItemID.BlueDye, ItemID.BlueandBlackDye, ItemID.BrightBlueDye, ItemID.BlueandSilverDye);

    public static BasicColorDye Purple { get; } = new(ItemID.PurpleDye, ItemID.PurpleandBlackDye, ItemID.BrightPurpleDye, ItemID.PurpleandSilverDye);

    public static BasicColorDye Violet { get; } = new(ItemID.VioletDye, ItemID.VioletandBlackDye, ItemID.BrightVioletDye, ItemID.VioletandSilverDye);

    public static BasicColorDye Pink { get; } = new(ItemID.PinkDye, ItemID.PinkandBlackDye, ItemID.BrightPinkDye, ItemID.PinkandSilverDye);

    public static BasicColorDye Brown { get; } = new(ItemID.BrownDye, ItemID.BrownAndBlackDye, ItemID.BrightBrownDye, ItemID.BrownAndSilverDye);

    public bool IsVanilla => true;

    public int ItemId => thisId;

    public DyeLidType LidType => DyeLidType.Color;

    public DyeOverlayType Overlays => DyeOverlayType.None;

    public BrightColorDye Bright { get; } = new(brightId);

    public SilverCompoundDye Silver { get; } = new(silverId);

    public BlackCompoundDye Black { get; } = new(blackId);
}

/// <summary>
///     Brightness-affecting dyes; Black and Silver.
/// </summary>
public sealed class BrightnessDye(int itemId) : IDyeDefinition
{
    public static BrightnessDye Silver { get; } = new(ItemID.SilverDye);

    public static BrightnessDye Black { get; } = new(ItemID.BlackDye);

    public bool IsVanilla => true;

    public int ItemId => itemId;

    public DyeLidType LidType => DyeLidType.Color;

    public DyeOverlayType Overlays => DyeOverlayType.None;
}

/// <summary>
///     Bright variants of basic colors.
/// </summary>
public sealed class BrightColorDye(int itemId) : IDyeDefinition
{
    public static BrightColorDye Red => BasicColorDye.Red.Bright;

    public static BrightColorDye Orange => BasicColorDye.Orange.Bright;

    public static BrightColorDye Yellow => BasicColorDye.Yellow.Bright;

    public static BrightColorDye Lime => BasicColorDye.Lime.Bright;

    public static BrightColorDye Green => BasicColorDye.Green.Bright;

    public static BrightColorDye Teal => BasicColorDye.Teal.Bright;

    public static BrightColorDye Cyan => BasicColorDye.Cyan.Bright;

    public static BrightColorDye SkyBlue => BasicColorDye.SkyBlue.Bright;

    public static BrightColorDye Blue => BasicColorDye.Blue.Bright;

    public static BrightColorDye Purple => BasicColorDye.Purple.Bright;

    public static BrightColorDye Violet => BasicColorDye.Violet.Bright;

    public static BrightColorDye Pink => BasicColorDye.Pink.Bright;

    public static BrightColorDye Brown => BasicColorDye.Brown.Bright;

    public static BrightColorDye Silver { get; } = new(ItemID.BrightSilverDye);

    public bool IsVanilla => true;

    public int ItemId => itemId;

    public DyeLidType LidType => DyeLidType.Bright;

    public DyeOverlayType Overlays => DyeOverlayType.None;
}

/// <summary>
///     Gradient dyes.
/// </summary>
public class GradientDye(int itemId, bool intense = false) : IDyeDefinition
{
    /// <summary>
    ///     Gradient dyes with Black and Silver compounds.
    /// </summary>
    public sealed class WithCompounds(int itemId, int silverId, int blackId) : GradientDye(itemId, intense: false)
    {
        public GradientCompoundDye Silver { get; } = new(silverId);

        public GradientCompoundDye Black { get; } = new(blackId);
    }

    public static WithCompounds Flame { get; } = new(ItemID.FlameDye, ItemID.FlameAndSilverDye, ItemID.FlameAndBlackDye);

    public static WithCompounds GreenFlame { get; } = new(ItemID.GreenFlameDye, ItemID.GreenFlameAndSilverDye, ItemID.GreenFlameAndBlackDye);

    public static WithCompounds BlueFlame { get; } = new(ItemID.BlueFlameDye, ItemID.BlueFlameAndSilverDye, ItemID.BlueFlameAndBlackDye);

    public static GradientDye YellowGradient { get; } = new(ItemID.YellowGradientDye);

    public static GradientDye CyanGradient { get; } = new(ItemID.CyanGradientDye);

    public static GradientDye VioletGradient { get; } = new(ItemID.VioletGradientDye);

    public static GradientDye Rainbow { get; } = new(ItemID.RainbowDye);

    public static GradientDye IntenseFlame { get; } = new(ItemID.IntenseFlameDye, intense: true);

    public static GradientDye IntenseGreenFlame { get; } = new(ItemID.IntenseGreenFlameDye, intense: true);

    public static GradientDye IntenseBlueFlame { get; } = new(ItemID.IntenseBlueFlameDye, intense: true);

    public static GradientDye IntenseRainbow { get; } = new(ItemID.IntenseRainbowDye, intense: true);

    public bool IsVanilla => true;

    public int ItemId => itemId;

    public DyeLidType LidType => intense ? DyeLidType.Intense : DyeLidType.Gradient;

    public DyeOverlayType Overlays => DyeOverlayType.None;
}

/// <summary>
///     Compound Silver dyes.
/// </summary>
public sealed class SilverCompoundDye(int itemId) : IDyeDefinition
{
    public static SilverCompoundDye Red => BasicColorDye.Red.Silver;

    public static SilverCompoundDye Orange => BasicColorDye.Orange.Silver;

    public static SilverCompoundDye Yellow => BasicColorDye.Yellow.Silver;

    public static SilverCompoundDye Lime => BasicColorDye.Lime.Silver;

    public static SilverCompoundDye Green => BasicColorDye.Green.Silver;

    public static SilverCompoundDye Teal => BasicColorDye.Teal.Silver;

    public static SilverCompoundDye Cyan => BasicColorDye.Cyan.Silver;

    public static SilverCompoundDye SkyBlue => BasicColorDye.SkyBlue.Silver;

    public static SilverCompoundDye Blue => BasicColorDye.Blue.Silver;

    public static SilverCompoundDye Purple => BasicColorDye.Purple.Silver;

    public static SilverCompoundDye Violet => BasicColorDye.Violet.Silver;

    public static SilverCompoundDye Pink => BasicColorDye.Pink.Silver;

    public static SilverCompoundDye Brown => BasicColorDye.Brown.Silver;

    public bool IsVanilla => true;

    public int ItemId => itemId;

    public DyeLidType LidType => DyeLidType.CompoundSilver;

    public DyeOverlayType Overlays => DyeOverlayType.None;
}

/// <summary>
///     Compound Black dyes.
/// </summary>
public sealed class BlackCompoundDye(int itemId) : IDyeDefinition
{
    public static BlackCompoundDye Red => BasicColorDye.Red.Black;

    public static BlackCompoundDye Orange => BasicColorDye.Orange.Black;

    public static BlackCompoundDye Yellow => BasicColorDye.Yellow.Black;

    public static BlackCompoundDye Lime => BasicColorDye.Lime.Black;

    public static BlackCompoundDye Green => BasicColorDye.Green.Black;

    public static BlackCompoundDye Teal => BasicColorDye.Teal.Black;

    public static BlackCompoundDye Cyan => BasicColorDye.Cyan.Black;

    public static BlackCompoundDye SkyBlue => BasicColorDye.SkyBlue.Black;

    public static BlackCompoundDye Blue => BasicColorDye.Blue.Black;

    public static BlackCompoundDye Purple => BasicColorDye.Purple.Black;

    public static BlackCompoundDye Violet => BasicColorDye.Violet.Black;

    public static BlackCompoundDye Pink => BasicColorDye.Pink.Black;

    public static BlackCompoundDye Brown => BasicColorDye.Brown.Black;

    public static BlackCompoundDye Silver { get; } = new(ItemID.SilverAndBlackDye);

    public bool IsVanilla => true;

    public int ItemId => itemId;

    public DyeLidType LidType => DyeLidType.CompoundBlack;

    public DyeOverlayType Overlays => DyeOverlayType.None;
}

/// <summary>
///     Compound gradient dyes.
/// </summary>
public sealed class GradientCompoundDye(int itemId) : IDyeDefinition
{
    // Black
    public static GradientCompoundDye FlameAndBlack => GradientDye.Flame.Black;

    public static GradientCompoundDye GreenFlameAndBlack => GradientDye.GreenFlame.Black;

    public static GradientCompoundDye BlueFlameAndBlack => GradientDye.BlueFlame.Black;

    // Silver
    public static GradientCompoundDye FlameAndSilver => GradientDye.Flame.Silver;

    public static GradientCompoundDye GreenFlameAndSilver => GradientDye.GreenFlame.Silver;

    public static GradientCompoundDye BlueFlameAndSilver => GradientDye.BlueFlame.Silver;

    public bool IsVanilla => true;

    public int ItemId => itemId;

    public DyeLidType LidType => DyeLidType.CompoundGradient;

    public DyeOverlayType Overlays => DyeOverlayType.None;
}

/// <summary>
///     Miscellaneous, special dyes.
/// </summary>
public sealed class SpecialDye(int itemId, DyeLidType lidType = DyeLidType.Special, bool animated = false) : IDyeDefinition
{
    // Technically a compound dye
    public static SpecialDye BlackAndWhite { get; } = new(ItemID.BlackAndWhiteDye);

    // Strange

    public static SpecialDye Acid { get; } = new(ItemID.AcidDye, animated: true);

    public static SpecialDye BlueAcid { get; } = new(ItemID.BlueAcidDye, animated: true);

    public static SpecialDye RedAcid { get; } = new(ItemID.RedAcidDye, animated: true);

    public static SpecialDye Chlorophyte { get; } = new(ItemID.ChlorophyteDye, lidType: DyeLidType.Glowing);

    public static SpecialDye Gel { get; } = new(ItemID.GelDye, animated: true);

    public static SpecialDye GlowingMushroom { get; } = new(ItemID.MushroomDye, lidType: DyeLidType.Glowing);

    public static SpecialDye Grim { get; } = new(ItemID.GrimDye, animated: true);

    public static SpecialDye Hades { get; } = new(ItemID.HadesDye, animated: true);

    public static SpecialDye BurningHades { get; } = new(ItemID.BurningHadesDye, animated: true);

    public static SpecialDye ShadowflameHades { get; } = new(ItemID.ShadowflameHadesDye, animated: true);

    public static SpecialDye LivingOcean { get; } = new(ItemID.LivingOceanDye, animated: true);

    public static SpecialDye LivingFlame { get; } = new(ItemID.LivingFlameDye, animated: true);

    public static SpecialDye LivingRainbow { get; } = new(ItemID.LivingRainbowDye, animated: true);

    public static SpecialDye Martian { get; } = new(ItemID.MartianArmorDye, lidType: DyeLidType.Glowing);

    public static SpecialDye MidnightRainbow { get; } = new(ItemID.MidnightRainbowDye, animated: true);

    public static SpecialDye Mirage { get; } = new(ItemID.MirageDye, animated: true);

    public static SpecialDye Negative { get; } = new(ItemID.NegativeDye);

    public static SpecialDye Pixie { get; } = new(ItemID.PixieDye, lidType: DyeLidType.Glowing);

    public static SpecialDye Phase { get; } = new(ItemID.PhaseDye, animated: true);

    public static SpecialDye PurpleOoze { get; } = new(ItemID.PurpleOozeDye, animated: true);

    public static SpecialDye Reflective { get; } = new(ItemID.ReflectiveDye, lidType: DyeLidType.Reflective);

    public static SpecialDye ReflectiveCopper { get; } = new(ItemID.ReflectiveCopperDye, lidType: DyeLidType.Reflective);

    public static SpecialDye ReflectiveGold { get; } = new(ItemID.ReflectiveGoldDye, lidType: DyeLidType.Reflective);

    public static SpecialDye ReflectiveObsidian { get; } = new(ItemID.ReflectiveObsidianDye, lidType: DyeLidType.Reflective);

    public static SpecialDye ReflectiveMetal { get; } = new(ItemID.ReflectiveMetalDye, lidType: DyeLidType.Reflective);

    public static SpecialDye ReflectiveSilver { get; } = new(ItemID.ReflectiveSilverDye, lidType: DyeLidType.Reflective);

    public static SpecialDye Shadow { get; } = new(ItemID.ShadowDye);

    public static SpecialDye ShiftingSands { get; } = new(ItemID.ShiftingSandsDye, animated: true);

    public static SpecialDye SkiphsBlood { get; } = new(ItemID.DevDye, animated: true);

    public static SpecialDye Twilight { get; } = new(ItemID.TwilightDye, animated: true);

    public static SpecialDye Wisp { get; } = new(ItemID.WispDye, lidType: DyeLidType.Glowing);

    public static SpecialDye InfernalWisp { get; } = new(ItemID.InfernalWispDye, lidType: DyeLidType.Glowing);

    public static SpecialDye UnicornWisp { get; } = new(ItemID.UnicornWispDye, lidType: DyeLidType.Glowing);

    // Crafted

    public static SpecialDye PinkGel { get; } = new(ItemID.PinkGelDye, animated: true);

    public static SpecialDye ShiftingPearlSands { get; } = new(ItemID.ShiftingPearlSandsDye, animated: true);

    public static SpecialDye Nebula { get; } = new(ItemID.NebulaDye, animated: true);

    public static SpecialDye Solar { get; } = new(ItemID.SolarDye, animated: true);

    public static SpecialDye Stardust { get; } = new(ItemID.StardustDye, animated: true);

    public static SpecialDye Vortex { get; } = new(ItemID.VortexDye, animated: true);

    public static SpecialDye Void { get; } = new(ItemID.VoidDye, animated: true);

    // Other

    public static SpecialDye Lokis { get; } = new(ItemID.LokisDye, animated: true);

    // public static SpecialDye SkiphsBlood { get; } = new(ItemID.DevDye, animated: true);

    public static SpecialDye Team { get; } = new(ItemID.TeamDye, lidType: DyeLidType.Player);

    public static SpecialDye BloodBath { get; } = new(ItemID.BloodbathDye, animated: true);

    public static SpecialDye Fogbound { get; } = new(ItemID.FogboundDye, animated: true);

    // public static SpecialDye Negative { get; } = new(ItemID.NegativeDye);

    // public static SpecialDye Shadow { get; } = new(ItemID.ShadowDye);

    public static SpecialDye Prismatic { get; } = new(ItemID.HallowBossDye, animated: true);

    // Reuses Unicorn Wisp Dye, so it's Glowing, I guess.
    public static SpecialDye ColorOnly { get; } = new(ItemID.ColorOnlyDye, DyeLidType.Glowing);

    public bool IsVanilla => true;

    public int ItemId => itemId;

    public DyeLidType LidType => lidType;

    public DyeOverlayType Overlays => animated ? DyeOverlayType.Animated : DyeOverlayType.None;
}

/// <summary>
///     Extensions to dye definitions.
/// </summary>
public static class DyeDefinitionExtensions
{
    extension(IDyeDefinition definition)
    {
        /// <summary>
        ///     Gets the shader ID from the item ID.
        /// </summary>
        public int ShaderId => GameShaders.Armor.GetShaderIdFromItemId(definition.ItemId);

        /// <summary>
        ///     Gets the shader from the item ID.
        /// </summary>
        public ArmorShaderData Shader => GameShaders.Armor.GetShaderFromItemId(definition.ItemId);
    }
}
