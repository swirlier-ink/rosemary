using System;

namespace Rosemary.Common;

// From NIGHTSHADE, with permission

public enum DyeLidType
{
    /// <summary>
    ///     For base color dyes; a black lid.
    /// </summary>
    Color,

    /// <summary>
    ///     For bright variants of base dyes; a white lid.
    /// </summary>
    Bright,

    /// <summary>
    ///     For gradient dyes; a desaturated blue/purple lid.
    /// </summary>
    Gradient,

    /// <summary>
    ///     For intense gradient dyes; a saturated blue/purple lid.
    /// </summary>
    Intense,

    /// <summary>
    ///     For black-compound dyes; a dotted, black lid.
    /// </summary>
    CompoundBlack,

    /// <summary>
    ///     For silver-compound dyes; a dotted, silver lid.
    /// </summary>
    CompoundSilver,

    /// <summary>
    ///     For both black- and silver-compound dyes when the compound color is
    ///     a gradient; a dotted, cyan lid.
    /// </summary>
    CompoundGradient,

    /// <summary>
    ///     For glowing dyes; a vibrant, green lid.
    /// </summary>
    Glowing,

    /// <summary>
    ///     For reflective dyes; a gold lid.
    /// </summary>
    Reflective,

    /// <summary>
    ///     For dyes that are affected by player attributes; a red lid.  In
    ///     vanilla, this only corresponds to the Team Dye.  Nightshade adds
    ///     more dyes that use this.
    /// </summary>
    Player,

    /// <summary>
    ///     For special dyes, typically animated; a bright, purple lid.
    /// </summary>
    Special,

    // MODDED LIDS
}

[Flags]
public enum DyeOverlayType
{
    /// <summary>
    ///     Represents no overlay.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Indicates this dye has an animation; a blue orb or sparkle over the
    ///     bottle.
    /// </summary>
    Animated = 1 << 0,

    // MODDED OVERLAYS
}
