using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria.GameContent.Generation;
using Terraria.WorldBuilding;
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace Rosemary.Common;

public static class GenPasses
{
    // GenPassNameID names provided as constants

    public const string Terrain = "Terrain";

    public const string Skyblock = "Skyblock";

    public const string DunesAndPyramidLocations = "Dunes";

    public const string OceanSand = "Ocean Sand";

    public const string SandPatches = "Sand Patches";

    public const string Tunnels = "Tunnels";

    public const string MountainCaves = "Mount Caves";

    public const string DirtWallBackgrounds = "Dirt Wall Backgrounds";

    public const string RocksInDirt = "Rocks In Dirt";

    public const string DirtInRocks = "Dirt In Rocks";

    public const string Clay = "Clay";

    public const string SmallHoles = "Small Holes";

    public const string DirtLayerCaves = "Dirt Layer Caves";

    public const string RockLayerCaves = "Rock Layer Caves";

    public const string SurfaceCaves = "Surface Caves";

    public const string WavyCaves = "Wavy Caves";

    public const string IceBiome = "Generate Ice Biome";

    public const string Grass = "Grass";

    public const string Jungle = "Jungle";

    public const string MudCavesToJungleGrass = "Mud Caves To Grass";

    public const string DesertBiome = "Full Desert";

    public const string GlowingMushroomPatches = "Mushroom Patches";

    public const string Marble = "Marble";

    public const string Granite = "Granite";

    public const string FloatingIslands = "Floating Islands";

    public const string DirtToMud = "Dirt To Mud";

    public const string Silt = "Silt";

    public const string OresAndShinies = "Shinies";

    public const string Webs = "Webs";

    public const string Underworld = "Underworld";

    public const string CorruptionAndCrimson = "Corruption";

    public const string Lakes = "Lakes";

    public const string StoneToIceAndSiltPlusMudIntoSlush = "Slush";

    public const string DualDungeonsDitherSnake = "Dual Dungeons Dither Snake";

    public const string Dungeon = "Dungeon";

    public const string MountainCaveOpenings = "Mountain Caves";

    public const string BeachesAndOceanCleanup = "Beaches";

    public const string Gems = "Gems";

    public const string GravitatingSandCleanup = "Gravitating Sand";

    public const string OceanCaves = "Create Ocean Caves";

    public const string Shimmer = "Shimmer";

    public const string DirtWallCleanup = "Clean Up Dirt";

    public const string Pyramids = "Pyramids";

    public const string DirtRockWallRunner = "Dirt Rock Wall Runner";

    public const string LivingTrees = "Living Trees";

    public const string LivingTreeWalls = "Wood Tree Walls";

    public const string DemonAndCrimsonAltars = "Altars";

    public const string SurfaceWaterInJungle = "Wet Jungle";

    public const string LihzahrdTemple = "Jungle Temple";

    public const string Beehives = "Hives";

    public const string JungleShrines = "Jungle Chests";

    public const string SettleLiquids = "Settle Liquids";

    public const string RemoveSurfaceWaterAboveSand = "Remove Water From Sand";

    public const string Oasis = "Oasis";

    public const string ShellPilesMarblePilesAndSpikePits = "Shell Piles";

    public const string SmoothWorld = "Smooth World";

    public const string Waterfalls = "Waterfalls";

    public const string FragileIceOverIceBiomeWater = "Ice";

    public const string CaveWallVariety = "Wall Variety";

    public const string LifeCrystals = "Life Crystals";

    public const string Statues = "Statues";

    public const string UndergroundHousesAndBuriedChests = "Buried Chests";

    public const string SurfaceChests = "Surface Chests";

    public const string ChestsInJungleShrines = "Jungle Chests Placement";

    public const string UnderwaterChests = "Water Chests";

    public const string SpiderCaves = "Spider Caves";

    public const string GemCaves = "Gem Caves";

    public const string MossAndMossCaves = "Moss";

    public const string LihzahrdTemplePart2 = "Temple";

    public const string CaveWallsInEnclosedSpaces = "Cave Walls";

    public const string UndergroundJungleTrees = "Jungle Trees";

    public const string FloatingIslandHouses = "Floating Island Houses";

    public const string QuickCleanup = "Quick Cleanup";

    public const string PotsGraveyardsAndBoulderPiles = "Pots";

    public const string Hellforges = "Hellforge";

    public const string SpreadingGrassOnSurfaceSunflowersEvilsOnSurfaceAndLavaCleanup = "Spreading Grass";

    public const string SurfaceOreAndStone = "Surface Ore and Stone";

    public const string FallenLogsAndWaterFeatures = "Place Fallen Log";

    public const string Traps = "Traps";

    public const string Piles = "Piles";

    public const string SpawnPoint = "Spawn Point";

    public const string SurfaceDirtWallsToGrassWalls = "Grass Wall";

    public const string SpawnStarterNPCs = "Guide";

    public const string SunflowersPart2 = "Sunflowers";

    public const string Trees = "Planting Trees";

    public const string AlchemyHerbs = "Herbs";

    public const string DyePlants = "Dye Plants";

    public const string WebsInSpiderCavesAndHoneyPlusSpeleothemsInBeehives = "Webs And Honey";

    public const string GrassPlantsEvilPlantsAndPumpkinsOnSurface = "Weeds";

    public const string GlowingMushroomPlantsUndergroundAndJunglePlants = "Glowing Mushrooms and Jungle Plants";

    public const string JunglePlantsPart2 = "Jungle Plants";

    public const string Vines = "Vines";

    public const string Flowers = "Flowers";

    public const string Mushrooms = "Mushrooms";

    public const string ExposedGemsInIceBiome = "Gems In Ice Biome";

    public const string ExposedGemsUnderground = "Random Gems";

    public const string LongMoss = "Moss Grass";

    public const string DirtWallsIntoMudWallsInJungleAndJungleMinMax = "Muds Walls In Jungle";

    public const string BeeLarvaInBeehives = "Larva";

    public const string SettleLiquidsPart2AndNotTheBees = "Settle Liquids Again";

    public const string CactusPalmTreesAndCoral = "Cactus, Palm Trees, & Coral";

    public const string TileCleanup = "Tile Cleanup";

    public const string LihzahrdAltar = "Lihzahrd Altars";

    public const string MicroBiomes = "Micro Biomes";

    public const string LilypadsCattailsBambooAndSeaweed = "Water Plants";

    public const string SpeleothemsAndGemTrees = "Stalac";

    public const string BrokenTrapCleanup = "Remove Broken Traps";

    public const string FinalCleanup = "Final Cleanup";

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    [HookMetadata(DelegateType = typeof(WorldGenLegacyMethod))]
    public class InsertAttribute(string targetPass, string name, float weight, bool after) : BaseHookAttribute
    {
        public readonly string TargetPass = targetPass;

        public readonly string Name = name;

        public readonly float Weight = weight;

        public readonly bool After = after;

        public override void Apply(MethodInfo bindingMethod, object? instance)
        {
            var method = HookSubscriber.BuildWrapper<WorldGenLegacyMethod>(bindingMethod, instance);

            var genPass = new PassLegacy(
                Name,
                method,
                Weight
            );

            passes.Add(
                new Pass(
                    genPass,
                    TargetPass,
                    After
                )
            );
        }
    }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    [HookMetadata(DelegateType = typeof(WorldGenLegacyMethod))]
    public sealed class BeforeAttribute(string targetPass, string name, float weight)
        : InsertAttribute(targetPass, name, weight, false);

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    [HookMetadata(DelegateType = typeof(WorldGenLegacyMethod))]
    public sealed class AfterAttribute(string targetPass, string name, float weight)
        : InsertAttribute(targetPass, name, weight, true);

    private record struct Pass(GenPass GenPass, string TargetPass, bool After);

    private static readonly List<Pass> passes = [];

    [ModSystemHooks.ModifyWorldGenTasks]
    private static void ModifyInterfaceLayers(List<GenPass> tasks)
    {
        foreach (var (pass, target, after) in passes)
        {
            var index = tasks.FindIndex(l => l.Name.Equals(target));

            if (index <= -1)
            {
                continue;
            }

            if (after)
            {
                index++;
            }

            tasks.Insert(
                index,
                pass
            );
        }
    }
}
