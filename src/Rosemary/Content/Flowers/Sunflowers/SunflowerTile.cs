using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Rosemary.Common;
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
    // TODO: Move to daybreak hooks
    private sealed class SunflowerGlobal : GlobalItem
    {
        public override bool AppliesToEntity(Item item, bool lateInstantiation) =>
            item.type == ItemID.Sunflower;

        public override void SetStaticDefaults()
        {
            // TODO: Better system for this, because this is stupid
            ItemID.Sets.DisableAutomaticPlaceableDrop[ItemID.Sunflower] = true;
        }

        public override void SetDefaults(Item item)
        {
            item.createTile = ModContent.TileType<SunflowerTile>();
        }
    }

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
        var targetRotation = (((float)Main.time / 54000f) * 2f) - 7.3f;

        if (!Main.IsItDay())
        {
            if (Main.remixWorld)
            {
                targetRotation = 0;
            }
            else
            {
                targetRotation = -7.3f;
            }
        }

        return targetRotation;
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
    }

    public override ushort GetMapOption(int i, int j)
    {
        var tile = Framing.GetTileSafely(i, j);
        var above = Framing.GetTileSafely(i, j - 1);

        return tile.TileFrameY == 0
            || !above.HasTile
            || above.TileType != tile.TileType
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

        return false;
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
        var tile = Framing.GetTileSafely(i, j);
        var above = Framing.GetTileSafely(i, j - 1);

        if (tile.TileFrameY == 18
         && below.HasTile && below.TileType == tile.TileType
         && (!above.HasTile || above.TileType != tile.TileType))
        {
            tile.TileFrameY = 0;
            tile.TileFrameX = (short)(18 * Rand.Next(5));
        }

        return !below.HasTile;
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

        if (above.HasTile || above.LiquidAmount > 0)
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
