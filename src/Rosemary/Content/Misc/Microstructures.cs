using System;
using Rosemary.Common;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Rosemary.Content.Misc;

file static class Microstructures
{
    [GenPasses.Before(GenPasses.Piles, $"{nameof(Rosemary)}: ROCK_PATCHES", 4.22f)]
    private static void RockPatchesPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = Mods.Rosemary.Content.Misc.WorldGen.RockPatches.GetTextValue();

        var count = ((Main.maxTilesX - 4200) / 2100) * 2;
        var placed = 0;
        var bail = 0;

        while (placed < count)
        {
            GeneratePatch();

            bail++;

            if (bail >= 100)
            {
                return;
            }
        }

        return;

        void GeneratePatch()
        {
            var start = Rand.Next(140, Main.maxTilesX - 140);

            var size = Rand.Next(6, 17);

            var validTiles = new int[]
            {
                TileID.Grass,
                TileID.Dirt,
                TileID.Stone,
                TileID.ClayBlock,
                TileID.JungleGrass,
                TileID.Mud,
            };

            var lastY = -1;

            var surfacePoints = new HashSet<Point>();

            for (var i = start - size; i < start + size; i += 3)
            {
                var min = 20;
                var max = Main.worldSurface + 20;

                // Vague flatness check
                if (lastY != -1)
                {
                    min = lastY - 7;
                    max = lastY + 6;
                }

                for (var j = min; j < max; j++)
                {
                    var tile = Framing.GetTileSafely(i, j);
                    var below = Framing.GetTileSafely(i, j + 1);

                    var aboveSolid = (tile.HasTile
                                   && Main.tileSolid[tile.TileType])
                                  || tile.LiquidAmount > 0;

                    if (aboveSolid
                     || !below.HasTile
                     || !validTiles.Contains(below.TileType)
                     || CheckClouds(i, j))
                    {
                        continue;
                    }

                    surfacePoints.Add(new Point(i, j));
                    lastY = j + 1;

                    break;
                }
            }

            if (surfacePoints.Count < ((size * 2f) / 3f) - 2f
             || surfacePoints.Count == 0)
            {
                return;
            }

            var index = 0;
            foreach (var (i, j) in surfacePoints)
            {
                // WorldUtils.Gen(new Point(i, j - 40), new Shapes.Rectangle(1, 4), new Actions.PlaceTile(TileID.RubyGemspark));

                index++;
                progress.Set(((double)placed / count) + Utils.Remap(index, 0, surfacePoints.Count, 0f, 1f / count));

                if (Rand.NextBool(4))
                {
                    continue;
                }

                switch (Rand.Next(3))
                {
                    case 0:
                    {
                        PlaceSpike(i, j);

                        if (Rand.NextBool())
                        {
                            PlaceSpike(i + Rand.NextDirection(), j + 2);
                        }
                    }
                    break;

                    case 1:
                    {
                        PlaceCutStone(i, j);
                    }
                    break;

                    case 2:
                    {
                        Rock2X2(i, j);
                    }
                    break;
                }
            }

            placed++;
            bail -= 40;

            return;

            void PlaceSpike(int i, int j)
            {
                var mossedUp = Rand.NextBool(3);

                var first = true;

                for (var y = j - Rand.Next(1, 3); y < j + 2; y++)
                {
                    WorldGen.KillTile(i, y);

                    WorldGen.PlaceTile(i, y, TileID.Stone, true, true);
                    if (mossedUp && NearAir(i, j))
                    {
                        WorldGen.PlaceTile(i, y, TileID.BrownMoss, true, true);
                    }

                    var tile = Framing.GetTileSafely(i, y);

                    if (first)
                    {
                        tile.Slope = Rand.NextBool() ? SlopeType.SlopeDownRight : SlopeType.SlopeDownLeft;
                    }

                    first = false;
                }
            }

            void PlaceCutStone(int i, int j)
            {
                i -= 1;
                j -= Rand.Next(1, 4);

                var dir = Rand.NextBool();

                var mossedUp = Rand.NextBool(3);

                for (var x = i; x < i + 3; x++)
                for (var y = j; y < j + 6; y++)
                {
                    var tile = Framing.GetTileSafely(x, y);

                    // Dig into the ground partially
                    if (tile.HasTile && Rand.NextBool(3))
                    {
                        break;
                    }

                    WorldGen.KillTile(x, y);

                    WorldGen.PlaceTile(x, y, TileID.Stone, true, true);
                    if (mossedUp && NearAir(i, j))
                    {
                        WorldGen.PlaceTile(x, y, TileID.BrownMoss, true, true);
                    }

                    var nX = x - i;
                    var nY = y - j;

                    if (dir)
                    {
                        nX = 2 - nX;
                    }

                    if (nX == 2
                     && nY == 0)
                    {
                        tile.IsHalfBlock = true;
                    }
                    else if (nX == nY && nX < 2)
                    {
                        tile.Slope = dir ? SlopeType.SlopeUpLeft : SlopeType.SlopeUpRight;
                    }
                    else if (nX == nY - 1 && nX < 2)
                    {
                        tile.Slope = dir ? SlopeType.SlopeDownRight : SlopeType.SlopeDownLeft;
                    }
                }
            }

            void Rock2X2(int i, int j)
            {
                var mossedUp = Rand.NextBool();

                if (Rand.NextBool())
                {
                    i -= 1;
                }

                for (var x = i; x < i + 2; x++)
                for (var y = j; y < j + 2; y++)
                {
                    WorldGen.KillTile(x, y);

                    WorldGen.PlaceTile(x, y, TileID.Stone, true, true);
                    if (mossedUp && NearAir(i, j))
                    {
                        WorldGen.PlaceTile(x, y, TileID.BrownMoss, true, true);
                    }
                }
            }
        }

        static bool NearAir(int i, int j)
        {
            var neighbors = new[]
            {
                Framing.GetTileSafely(i - 1, j),
                Framing.GetTileSafely(i + 1, j),
                Framing.GetTileSafely(i, j - 1),
                Framing.GetTileSafely(i, j + 1),
            };

            return neighbors.Any(t => !t.HasTile || !Main.tileSolid[t.TileType]);
        }

        static bool CheckClouds(int i, int j)
        {
            for (var y = j; y < j + 30; y++)
            {
                var tile = Framing.GetTileSafely(i, y);

                if (tile.HasTile && TileID.Sets.Clouds[tile.TileType])
                {
                    return true;
                }
            }

            return false;
        }
    }
}
