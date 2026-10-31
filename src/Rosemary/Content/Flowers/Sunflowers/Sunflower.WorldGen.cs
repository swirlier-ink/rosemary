using FullSerializer.Internal;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Rosemary.Common;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.WorldBuilding;

namespace Rosemary.Content.Flowers;

public static class SunflowerWorldGen
{
    [OnLoad]
    private static void Load()
    {
        WorldGen.ModifyPass((PassLegacy)WorldGen.VanillaGenPasses[GenPassNameID.SpreadingGrassOnSurfaceSunflowersEvilsOnSurfaceAndLavaCleanup], UseModifiedSunflowers);
        WorldGen.ModifyPass((PassLegacy)WorldGen.VanillaGenPasses[GenPassNameID.SunflowersPart2], UseModifiedSunflowers);

        On_WorldGenerator.GenerateWorld += GenerateWorld_TempSunflowers;

        IL_WorldGen.SpreadGrass += SpreadGrass_RosemarySunflowers;
    }

    private static void SpreadGrass_RosemarySunflowers(ILContext il)
    {
        var c = new ILCursor(il);

        while (c.TryGotoNext(
                MoveType.After,
                i => i.MatchCall<Tile>($"get_{nameof(Tile.type)}"),
                i => i.MatchLdindU2(),
                i => i.MatchLdcI4(27)
            ))
        {
            c.EmitPop();
            c.EmitStaticDelegateUnsafe(static () => ModContent.TileType<SunflowerTile>());
        }
    }

    private static bool GenerateWorld_TempSunflowers(On_WorldGenerator.orig_GenerateWorld orig, WorldGenerator self)
    {
        var type = ModContent.TileType<SunflowerTile>();

        Main.tileCut[type] = false;
        Main.tileSolid[type] = true;

        var ret = orig(self);

        Main.tileCut[type] = true;
        Main.tileSolid[type] = false;

        return ret;
    }

    private static void UseModifiedSunflowers(ILContext il)
    {
        var c = new ILCursor(il);

        var iIndex = VariableIndex.Invalid;
        var jIndex = VariableIndex.Invalid;

        var jumpPlaceTileLabel = c.DefineLabel();

        c.GotoNext(
            MoveType.After,
            i => i.MatchCall<WorldGen>(nameof(WorldGen.PlaceTile)),
            i => i.MatchPop()
        );

        c.MarkLabel(jumpPlaceTileLabel);

        {
            var c2 = c.Clone();

            c2.GotoPrev(
                MoveType.Before,
                i => i.MatchLdloc(out iIndex),
                i => i.MatchLdloc(out jIndex),
                i => i.MatchLdcI4(1),
                i => i.MatchSub()
            );

            c2.EmitBr(jumpPlaceTileLabel);
        }

        c.EmitLdloc(iIndex);
        c.EmitLdloc(jIndex);
        c.EmitLdcI4(1);
        c.EmitSub();
        c.EmitDelegate(
            static (int i, int j) =>
            {
                if (Rand.NextBool(6))
                {
                    return;
                }
                
                var height = Rand.Next(3, 12);

                var type = ModContent.TileType<SunflowerTile>();

                var counter = 0;

                while (ValidTile(i, j) && counter <= height)
                {
                    var tile = Framing.GetTileSafely(i, j);

                    if (counter == 0)
                    {
                        var below = Framing.GetTileSafely(i, j + 1);

                        below.ClearSlope();

                        if (Main.tileCut[tile.TileType])
                        {
                            WorldGen.KillTile(i, j, noItem: true);
                        }

                        if (!WorldGen.PlaceTile(i, j, type, true))
                        {
                            return;
                        }
                    }

                    tile.HasTile = true;
                    tile.TileType = (ushort)type;
                    tile.TileFrameY = counter == 0 ? (short)(18 * 2) : (short)0;
                    tile.TileFrameX = (short)(18 * Rand.Next(5));

                    if (counter >= 2)
                    {
                        var below = Framing.GetTileSafely(i, j + 1);

                        below.TileFrameY = 18;
                    }

                    counter++;

                    if (counter > height && Rand.NextBool(5))
                    {
                        tile.TileFrameY = 18;
                        tile.TileFrameX = 18 * 5;
                    }

                    j--;
                }

                return;

                static bool ValidTile(int i, int j)
                {
                    var tile = Framing.GetTileSafely(i, j);

                    return !tile.HasWall
                        && tile.LiquidAmount <= 0
                        && (!tile.HasTile
                         || !Main.tileSolid[tile.TileType]
                         || Main.tileCut[tile.TileType]);
                }
            }
        );
    }
}
