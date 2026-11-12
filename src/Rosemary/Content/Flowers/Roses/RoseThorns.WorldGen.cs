using Microsoft.Xna.Framework;
using Rosemary.Common;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Rosemary.Content.Flowers;

public static class RoseThornsWorldGen
{
    [GenPasses.Before(GenPasses.AlchemyHerbs, $"{nameof(Rosemary)}: ZENITH_THORNS", 7.32f)]
    private static void ZenithThornsPass(GenerationProgress progress, GameConfiguration configuration)
    {
        if (!WorldGen.everythingWorldGen)
        {
            return;
        }

        progress.Message = Mods.Rosemary.Content.Misc.ZenithSeedThornsGeneration.GetTextValue();

        const string text = "GETFIXEDBOI";

        var count = Rand.Next(4, 8);
        var placed = 0;
        var bail = 0;

        while (placed < count)
        {
            GenerateString();

            bail++;

            if (bail >= 3000)
            {
                return;
            }
        }

        return;

        void GenerateString()
        {
            var length = TileFont.MeasureString(text).X + 80;

            var starting = Rand.Next(140, Main.maxTilesX - 140) - (length / 2);

            var index = 0;

            var lastY = -1;

            var pointsToPlaceLetters = new HashSet<Point>();

            for (var i = starting; i < starting + length; i++)
            {
                for (var j = 10; j < Main.maxTilesY; j++)
                {
                    if (Math.Abs(j - lastY) > 60 && index > 0)
                    {
                        continue;
                    }

                    if (!Rand.NextBool(4) && index == 0)
                    {
                        continue;
                    }

                    var below = Framing.GetTileSafely(i, j);

                    if (!below.HasTile
                     || !below.Slope.Block
                     || below.IsHalfBlock
                     || below.TileType != TileID.Grass)
                    {
                        continue;
                    }

                    var size = TileFont.MeasureString(text[index].ToString());

                    var start = new Point(i - (size.X / 2), j - size.Y);

                    if (CheckArea(start, size) < 18)
                    {
                        continue;
                    }

                    pointsToPlaceLetters.Add(start);

                    lastY = j;

                    index++;

                    i += 5;

                    if (index >= text.Length)
                    {
                        goto ESCAPE;
                    }
                }
            }

            if (index < text.Length)
            {
                return;
            }

        ESCAPE:

            index = 0;

            foreach (var point in pointsToPlaceLetters)
            {
                progress.Set(((double)placed / count) + Utils.Remap(index, starting, starting + length, 0f, 1f / count));

                GenerateLetter(point, text[index]);

                index++;
            }

            bail -= 600;
            placed++;
        }

        static int CheckArea(Point start, Point size)
        {
            var count = 0;

            for (var i = start.X; i < start.X + size.X; i++)
            for (var j = start.Y; j < start.Y + size.Y; j++)
            {
                var tile = Framing.GetTileSafely(i, j);

                if (tile.HasTile
                 && tile.TileType == ModContent.TileType<RoseThorn>())
                {
                    return 0;
                }

                if ((tile.HasTile
                  && !Main.tileCut[tile.TileType])
                 || tile.LiquidAmount > 0)
                {
                    continue;
                }

                count++;
            }

            return count;
        }

        static void GenerateLetter(Point topLeft, char @char)
        {
            var data = TileFont.MicroFont[@char];

            for (var j = 0; j < data.Length; j++)
            {
                var num = data[j] << 1;
                for (var i = 0; i < 5; i++)
                {
                    if ((num & 0x80) == 128)
                    {
                        var tile = Framing.GetTileSafely(topLeft.X + i, topLeft.Y + j);

                        if (!tile.HasTile || Main.tileCut[tile.TileType])
                        {
                            WorldGen.PlaceTile(topLeft.X + i, topLeft.Y + j, ModContent.TileType<RoseThorn>(), true);
                        }
                    }
                    num <<= 1;
                }
            }
        }
    }

    [GlobalTileHooks.RandomUpdate]
    private static void RandomUpdate(int i, int j, int type, bool underground)
    {
        var seedCon = Main.remixWorld || Main.zenithWorld;

        var standardCon = !underground
                       && !Main.hardMode
                       && Main.IsItDay()
                       && (i < Main.maxTilesX / 3
                        || i > Main.maxTilesX - (Main.maxTilesX / 3));

        if (!(standardCon || seedCon)
         || type != TileID.Grass
         || !Rand.NextBool(450))
        {
            return;
        }

        var tile = Framing.GetTileSafely(i, j - 1);
        var below = Framing.GetTileSafely(i, j);

        if (!below.HasTile
         || !below.Slope.Block
         || below.IsHalfBlock
         || tile.LiquidAmount > 0)
        {
            return;
        }

        if (tile.HasTile
         && tile.TileType != TileID.Plants
         && tile.TileType != TileID.Plants2)
        {
            return;
        }

        WorldGen.PlaceTile(i, j - 1, ModContent.TileType<RoseThorn>(), true);
        tile.CopyPaintAndCoating(below);

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendTileSquare(-1, i, j);
        }
    }
}
