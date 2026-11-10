using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Content.Misc;

public static partial class MiscProjectileSets
{
    // Should be noted that this works above the encumbering stone and will disable pickups for hearts and mana.
    private static bool[] affectsWindGrid = [];

    private static Mod Mod => ModContent.GetInstance<ModImpl>();

    [ModSystemHooks.ResizeArrays]
    private static void ResizeArrays()
    {
        // Based on CanDistortWater & NoLiquidDistortion
        affectsWindGrid = CreateSet(nameof(affectsWindGrid), true,
            ProjectileID.VilethornBase, false,
            ProjectileID.VilethornTip, false,
            ProjectileID.NettleBurstLeft, false,
            ProjectileID.NettleBurstRight, false,
            ProjectileID.NettleBurstEnd, false,
            ProjectileID.CrystalVileShardShaft, false,
            ProjectileID.CrystalVileShardHead, false,
            ProjectileID.ToxicCloud, false,
            ProjectileID.ToxicCloud2, false,
            ProjectileID.ToxicCloud3, false
        );

        return;

        static T[] CreateSet<T>(string name, T defaultState, params object[] inputs)
        {
            return ProjectileID.Sets.Factory.CreateNamedSet(Mod, name)
                         .RegisterCustomSet(defaultState, inputs);
        }
    }

    extension(ProjectileID.Sets)
    {
        public static bool[] AffectsWindGrid => affectsWindGrid;
    }
}
