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
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Rosemary.Content.Flowers;

public sealed class SunflowerTile : ModTile
{
    public override string Texture => Assets.Flowers.Sunflowers.SunflowerTile.KEY;

    public override void Load()
    {
        IL_TileDrawing.DrawRisingVineStrip += DrawRisingVineStrip_DrawSunflowerHead;
        On_WorldFile.SetOngoingToTemps += SetOngoingToTemps_ResetSunflowerRotation;
    }

    private static void SetOngoingToTemps_ResetSunflowerRotation(On_WorldFile.orig_SetOngoingToTemps orig)
    {
        orig();

        var targetRotation = GetTargetRotation();

        sunRotation = targetRotation;
    }

    private static void DrawRisingVineStrip_DrawSunflowerHead(ILContext il)
    {
        var c = new ILCursor(il);

        var rendererIndex = ParameterIndex.Invalid;

        var iIndex = ParameterIndex.Invalid;
        var jIndex = VariableIndex.Invalid;

        var textureIndex = VariableIndex.Invalid;
        var positionIndex = VariableIndex.Invalid;
        var colorIndex = VariableIndex.Invalid;
        var rotationIndex = VariableIndex.Invalid;
        var tileSpriteEffectsIndex = VariableIndex.Invalid;

        c.GotoNext(
            MoveType.After,
            i => i.MatchLdarg(out iIndex),
            i => i.MatchLdloc(out jIndex),
            i => i.MatchCall<Tilemap>("get_Item")
        );

        c.GotoNext(
            MoveType.After,
            i => i.MatchCall<TileDrawing>(nameof(TileDrawing.DrawNature))
        );

        // Find permits gaps, riskier
        {
            var c2 = c.Clone();

            c2.GotoPrev(
                i => i.MatchLdarg(out rendererIndex),
                i => i.MatchLdloc(out textureIndex),
                i => i.MatchLdloc(out positionIndex)
            );

            c2.GotoNext(
                i => i.MatchNewobj<Rectangle>(),
                i => i.MatchLdloc(out colorIndex),
                i => i.MatchLdloc(out rotationIndex)
            );

            c2.GotoNext(
                i => i.MatchNewobj<Vector2>(),
                i => i.MatchLdcR4(1f),
                i => i.MatchLdloc(out tileSpriteEffectsIndex)
            );
        }

        c.EmitLdarg(rendererIndex);
        c.EmitLdloc(textureIndex);
        c.EmitLdarg(iIndex);
        c.EmitLdloc(jIndex);
        c.EmitLdloc(positionIndex);
        c.EmitLdloc(colorIndex);
        c.EmitLdloc(rotationIndex);
        c.EmitLdloc(tileSpriteEffectsIndex);

        c.EmitDelegate(
            static (
                TileDrawing renderer,
                Texture2D texture,
                int i,
                int j,
                Vector2 position,
                Color color,
                float baseRotation,
                SpriteEffects effects
            ) =>
            {
                var tile = Framing.GetTileSafely(i, j);
                var above = Framing.GetTileSafely(i, j - 1);

                var bud = tile.TileFrameY == 18 * 2 && !(above.HasTile && above.TileType == tile.TileType);

                if (tile.TileType != ModContent.TileType<SunflowerTile>()
                 || tile.TileFrameX == 18 * 5
                 || !(tile.TileFrameY == 0 || bud))
                {
                    return;
                }

                position += new Vector2(i % 2 == 0 ? -1 : 1, -15).RotatedBy(baseRotation);

                var xFrameNormal = tile.TileFrameX / 18;

                var frame = new Rectangle(34 * xFrameNormal, 56, 34, 22);
                var origin = frame.Size() * 0.5f;

                var flags = SideFlags.None;
                var rotation = sunRotation;

                if (bud)
                {
                    frame = new Rectangle(0, 80, 34, 30);
                    origin.Y = 25;

                    flags |= SideFlags.Bottom;
                    rotation = 0f;

                    position += new Vector2(0, xFrameNormal * 4).RotatedBy(baseRotation);
                }

                rotation += baseRotation;

                renderer.DrawNature(texture, position, frame, color, rotation, origin, 1f, effects, 0f, flags);
            }
        );
    }

    private static float sunRotation;

    [ModSystemHooks.PostUpdateTime]
    private static void PostUpdateTime()
    {
        var targetRotation = GetTargetRotation();

        if (Math.Abs(sunRotation - targetRotation) > 0.02f)
        {
            sunRotation += 0.001f * (sunRotation < targetRotation ? 1f : -1f);
        }
    }

    private static float GetTargetRotation()
    {
        if (Main.IsItDay())
        {
            if (Main.eclipse)
            {
                return -7.3f;
            }

            return (((float)Main.time / 54000f) * 2f) - 7.3f;
        }

        if (Main.remixWorld)
        {
            return 0f;
        }

        if (!Main.dayTime
         && Main.bloodMoon
         && Main.moonPhase == (int)MoonPhase.Full)
        {
            return ((float)Main.time / 32400f) * 2f - 7.3f;
        }

        return -7.3f;
    }

    public override void SetStaticDefaults()
    {
        Main.tileCut[Type] = true;
        Main.tileBlockLight[Type] = false;
        Main.tileLavaDeath[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;

        TileID.Sets.TileCutIgnore.Regrowth[Type] = true;
        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.ReverseVineThreads[Type] = true;
        TileID.Sets.MultiTileSway[Type] = true;
        TileID.Sets.DoesntGetReplacedWithTileReplacement[Type] = true;
        TileID.Sets.IsMultitile[Type] = true;
        TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;
        TileID.Sets.BreakableWhenPlacing[Type] = false;
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
        TileID.Sets.SpawnsNatureFlies[Type] = true;
        TileID.Sets.SlowlyDiesInWater[Type] = true;

        // Vanilla sunflowers reduce evil biome counts, should tall
        // sunflowers reduce them by less to compensate for their height?
        TileID.Sets.CorruptBiome[Type] = -5;
        TileID.Sets.CrimsonBiome[Type] = -5;
        TileID.Sets.RemixCorruptBiome[Type] = -5;
        TileID.Sets.RemixCrimsonBiome[Type] = -5;

        this.NewTileData(TileObjectData.Style1x1)
            .LavaDeath(true)
            .WaterPlacement(LiquidPlacement.NotAllowed)
            .AnchorBottom(new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 1, 0))
            .AnchorValidTiles(
                 TileID.Grass,
                 TileID.GolfGrass,
                 TileID.HallowedGrass,
                 TileID.JungleGrass,
                 TileID.GolfGrassHallowed,
                 TileID.AshGrass
             )
            .Register();

        DustType = DustID.Grass;
        HitSound = SoundID.Grass;

        AddMapEntry(new Color(54, 154, 54), Lang.GetItemName(ItemID.Sunflower));
        AddMapEntry(new Color(226, 196, 49), Lang.GetItemName(ItemID.Sunflower));

        RegisterItemDrop(ItemID.None);
    }

    public override ushort GetMapOption(int i, int j)
    {
        var tile = Framing.GetTileSafely(i, j);

        return tile.TileFrameY == 0
             ? (ushort)1
             : (ushort)0;
    }

    public override void NearbyEffects(int i, int j, bool closer)
    {
        if (closer)
        {
            return;
        }

        Main.SceneMetrics.HasSunflower = true;
    }

    public override bool PreDraw(int i, int j, SpriteBatch sb)
    {
        TileDrawing.Instance.CrawlToBottomOfReverseVineAndAddSpecialPoint(j, i);

        if (!TileDrawing.Instance._isActiveAndNotPaused || !Rand.NextBoolean(4))
        {
            return false;
        }

        // For whatever reason TileDrawing.EmitParticles doesn't work correctly here?
        // No matter though, since a bit of custom logic is preferable

        var tile = Framing.GetTileSafely(i, j);

        if (tile.TileFrameY != 0)
        {
            return false;
        }

        var lerpValue = Utils.GetLerpValue(0.08f, 0.18f, Math.Abs(Main.WindForVisuals), clamped: true);
        lerpValue += 0.3f;

        if (Rand.Next(1f) >= lerpValue)
        {
            return false;
        }

        var flag2 = Rand.Next(400) == 0; // 600

        if (!flag2)
        {
            TileDrawing.Instance._windGrid.GetWindTime(i, j, 8, out var windTimeLeft, out _, out _);
            flag2 = windTimeLeft > 0 && Rand.Next(48) == 0;
        }

        if (flag2)
        {
            ParticleOrchestrator.RequestParticleSpawn(
                clientOnly: true,
                ParticleOrchestraType.NatureFly,
                new ParticleOrchestraSettings
                {
                    PositionInWorld = new Point(i, j).ToWorldCoordinates(new Vector2(8, 0)),
                }
            );
        }

        return false;
    }

    public override bool PreDrawPlacementPreview(int i, int j, SpriteBatch spriteBatch, ref Rectangle frame, ref Vector2 position, ref Color color, bool validPlacement, ref SpriteEffects spriteEffects)
    {
        return false;
    }

    public override bool CanPlace(int i, int j)
    {
        var tile = Framing.GetTileSafely(i, j);

        if ((j > Main.worldSurface - 1 && !Main.remixWorld)
         || tile.HasWall)
        {
            return false;
        }

        return true;
    }

    public override void PlaceInWorld(int i, int j, Item item)
    {
        var tile = Framing.GetTileSafely(i, j);

        tile.TileFrameY = 18 * 2;
        tile.TileFrameX = (short)(18 * Rand.Next(5));

        NetMessage.SendTileSquare(-1, i, j);
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        tileFrameX = (short)((tileFrameX / 18) * 34);
        width = 32;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
    {
        if (i % 2 == 0)
        {
            spriteEffects = SpriteEffects.FlipHorizontally;
        }
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        var below = Framing.GetTileSafely(i, j + 1);

        return !below.HasTile;
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        j++;

        var below = Framing.GetTileSafely(i, j + 1);
        var tile = Framing.GetTileSafely(i, j);

        if (tile.TileType != Type)
        {
            return;
        }

        // Stunted frames
        if (tile.TileFrameY == 18
         && below.HasTile && below.TileType == tile.TileType)
        {
            tile.TileFrameY = 18;
            tile.TileFrameX = 18 * 5;
        }

        if (tile.TileFrameY == 18 * 2)
        {
            tile.TileFrameY = 18 * 2;
            tile.TileFrameX = 18 * 5;
        }
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j)
    {
        var tile = Framing.GetTileSafely(i, j);

        if (tile.TileFrameY == 18 * 2 || Rand.NextBoolean(5))
        {
            yield return new Item(ItemID.Sunflower);
        }
    }

    public override void RandomUpdate(int i, int j, bool underground)
    {
        if (underground
         || !Rand.NextBoolean(6))
        {
            return;
        }

        var tile = Framing.GetTileSafely(i, j);
        var above = Framing.GetTileSafely(i, j - 1);
        var below = Framing.GetTileSafely(i, j + 1);

        if (above.HasTile
         || above.LiquidAmount > 0
         || tile.TileFrameX == 18 * 5) // Broken frame should "stunt" growth
        {
            return;
        }

        var maxHeight = (Math.Abs(i % 3 + i % 23 - i) % 7) + 5;

        for (var k = 0; k <= maxHeight; ++k)
        {
            var cur = Framing.GetTileSafely(i, j + k);

            if (!cur.HasTile || cur.TileType != tile.TileType)
            {
                break;
            }

            if (k >= maxHeight)
            {
                return;
            }
        }

        if (below.TileType == tile.TileType)
        {
            tile.TileFrameY = 18;
            tile.TileFrameX = (short)(18 * Rand.Next(5));
        }

        above.TileType = tile.TileType;
        above.TileFrameY = 0;
        above.TileFrameX = (short)(18 * Rand.Next(5));

        above.HasTile = true;
        above.CopyPaintAndCoating(tile);

        WorldGen.SquareTileFrame(i, j);

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendTileSquare(-1, i, j);
        }
    }
}
