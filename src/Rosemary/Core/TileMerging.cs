using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Rosemary.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rosemary.Core;

// Adapted from 'https://github.com/GabeHasWon/SpiritReforged/blob/scarabdate/Common/TileCommon/TileMerging/TileMerger.cs' with permission from math2.
public static class TileMerging
{
    private static readonly Dictionary<int, Asset<Texture2D>> texture_overlay_by_type = [];

    private static Dictionary<int, HashSet<int>> MergesWith { get; } = [];

    private static readonly Point[] offsets =
    [
        new(-1, -1), new(18, 0), new(18, 36), new(54, 0),
        new(0, 18), new(0, 0), new(0, 36), new(90, 0),
        new(36, 18), new(36, 0), new(36, 36), new(90, 18),
        new(54, 18), new(72, 0), new(72, 18), new(18, 18),
    ];

    private static readonly Point[] corner_offsets =
    [
        new(-1, -1), new(18, 90), new(18, 72), new(36, 54),
        new(0, 90), new(0, 54), new(18, 108), new(54, 90),
        new(0, 72), new(0, 108), new(18, 54), new(54, 72),
        new(54, 54), new(36, 90), new(36, 72), new(36, 108),
    ];

    private static bool[] usesCornerMergeFrames = [];

    private static Mod Mod => ModContent.GetInstance<ModImpl>();

    [ModSystemHooks.ResizeArrays]
    private static void ResizeArrays()
    {
        usesCornerMergeFrames = CreateSet(nameof(usesCornerMergeFrames), false);

        return;

        static T[] CreateSet<T>(string name, T defaultState)
        {
            return TileID.Sets.Factory.CreateNamedSet(Mod, name)
                         .RegisterCustomSet(defaultState);
        }
    }

    /// <summary>
    ///     Adds custom merge frame overlays for supplied <paramref name="targetTypes"/>;
    ///     uses a texture format different to that of vanilla dirt merge frames.
    ///     <br/>
    ///     Additionally, merge overlays will use the paint of the neighboring
    ///     tiles over the paint of the center tile.
    /// </summary>
    public static void AddCustomMerge(int type, Asset<Texture2D> texture, params int[] targetTypes)
    {
        texture_overlay_by_type[type] = texture;

        TileID.Sets.ChecksForMerge[type] = true;

        foreach (var targetType in targetTypes)
        {
            MergesWith.TryAdd(targetType, []);
            MergesWith[targetType].Add(type);

            Main.tileMerge[type][targetType] = true;
            Main.tileMerge[targetType][type] = true;

            TileID.Sets.ChecksForMerge[targetType] = true;
        }
    }

    /// <inheritdoc cref="AddCustomMerge(int, Asset{Texture2D}, int[])"/>
    public static void AddCustomMerge(int type, bool useCorners, Asset<Texture2D> texture, params int[] targetTypes)
    {
        texture_overlay_by_type[type] = texture;

        TileID.Sets.ChecksForMerge[type] = true;

        usesCornerMergeFrames[type] = useCorners;

        foreach (var targetType in targetTypes)
        {
            MergesWith.TryAdd(targetType, []);
            MergesWith[targetType].Add(type);

            Main.tileMerge[type][targetType] = true;
            Main.tileMerge[targetType][type] = true;

            TileID.Sets.ChecksForMerge[targetType] = true;
        }
    }

#region Drawing
    [GlobalTileHooks.PostDraw]
    private static void PostDraw(int i, int j, int type, [OriginalName("spriteBatch")] SpriteBatch sb)
    {
        var tile = Framing.GetTileSafely(i, j);

        if (tile.HasTile
         && TileDrawing.IsVisible(tile)
         && MergesWith.TryGetValue(type, out var merges)
         && merges.Count > 0)
        {
            DrawMerge(sb, i, j, merges);
        }
    }

    private static void DrawMerge(SpriteBatch sb, int i, int j, params IEnumerable<int> types)
    {
        const int full_frame_width = 108;

        var tile = Framing.GetTileSafely(i, j);

        var frameNumber = tile.Get<TileWallWireStateData>().TileFrameNumber;

        var off = new Vector2(Main.offScreenRange);

        var position = new Vector2(i * 16, j * 16) - Main.screenPosition + off;

        foreach (var type in types)
        {
            var (mask, paintType, fullBright) = GetMergeData(i, j, type);

            if (usesCornerMergeFrames[type])
            {
                DrawCorners(type, mask);
            }

            if (mask <= 0 ||
                !texture_overlay_by_type.TryGetValue(type, out var asset))
            {
                continue;
            }

            Texture2D? texture = null;

            var useColor = paintType > PaintID.None && !TryGetPaintTexture(type, paintType, asset, out texture);

            texture ??= asset.Value;

            var color = GetTileColor(type, useColor, paintType, fullBright);

            var p = offsets[mask];

            var source = new Rectangle(p.X + (frameNumber * full_frame_width), p.Y, 16, 16);

            sb.Draw(texture, position, source, color);
        }

        return;

        void DrawCorners(int type, int edgeMask)
        {
            var (mask, paintType, fullBright) = GetMergeCornerData(i, j, type, edgeMask);

            if (mask <= 0 ||
                !texture_overlay_by_type.TryGetValue(type, out var asset))
            {
                return;
            }

            Texture2D? texture = null;

            var useColor = paintType > PaintID.None && !TryGetPaintTexture(type, paintType, asset, out texture);

            texture ??= asset.Value;

            var color = GetTileColor(type, useColor, paintType, fullBright);

            var p = corner_offsets[mask];

            var source = new Rectangle(p.X + (frameNumber * full_frame_width), p.Y, 16, 16);

            sb.Draw(texture, position, source, color);
        }

        Color GetTileColor(int type, bool useColor, int paintType, bool fullBright)
        {
            var tileColor = Lighting.GetColor(i, j);

            if (fullBright)
            {
                tileColor = Color.White;
            }

            if (tile.IsActuated)
            {
                tileColor = tile.actColor(tileColor);
            }
            else if (TileDrawing.ShouldTileShine((ushort)type, tile.frameX))
            {
                tileColor = Main.shine(tileColor, type);
            }

            if (useColor)
            {
                tileColor = tileColor.MultiplyRGBA(WorldGen.paintColor(paintType));
            }

            return tileColor;
        }
    }

    private static (int Mask, int PaintType, bool FullBright) GetMergeData(int i, int j, int type)
    {
        var center = Framing.GetTileSafely(i, j);

        var mask = 0;
        var paintType = 0;
        var fullBright = false;

        // Check for each tile merging with its neighbor;
        // tiles should only merge if their slope state allows it.

        var down = Framing.GetTileSafely(i, j + 1);
        if (center.Slope.Down && down.Slope.Up && !down.IsHalfBlock)
        {
            Check(down, 2);
        }

        // Half tiles should only merge with the tile below them.
        if (center.IsHalfBlock)
        {
            return (mask, paintType, fullBright);
        }

        var up = Framing.GetTileSafely(i, j - 1);
        if (center.Slope.Up && up.Slope.Down)
        {
            Check(up, 1);
        }

        var left = Framing.GetTileSafely(i - 1, j);
        if (center.Slope.Left && left.Slope.Right && !left.IsHalfBlock)
        {
            Check(left, 4);
        }

        var right = Framing.GetTileSafely(i + 1, j);
        if (center.Slope.Right && right.Slope.Left && !right.IsHalfBlock)
        {
            Check(right, 8);
        }

        return (mask, paintType, fullBright);

        void Check(Tile tile, int bit)
        {
            if (tile.TileType != type || !TileDrawing.IsVisible(tile))
            {
                return;
            }

            mask |= bit;

            if (paintType == 0)
            {
                paintType = tile.TileColor;
            }

            fullBright = tile.IsTileFullbright;
        }
    }

    private static (int Mask, int PaintType, bool FullBright) GetMergeCornerData(int i, int j, int type, int edgeMask)
    {
        var center = Framing.GetTileSafely(i, j);

        var mask = 0;
        var paintType = 0;
        var fullBright = false;

        var downLeft = Framing.GetTileSafely(i - 1, j + 1);
        if (center.Slope.DownLeft && downLeft.Slope.UpRight && !downLeft.IsHalfBlock
         && (edgeMask & 2) == 0 && (edgeMask & 4) == 0)
        {
            Check(downLeft, 2);
        }

        var downRight = Framing.GetTileSafely(i + 1, j + 1);
        if (center.Slope.DownRight && downLeft.Slope.UpLeft && !downLeft.IsHalfBlock
         && (edgeMask & 2) == 0 && (edgeMask & 8) == 0)
        {
            Check(downRight, 8);
        }

        if (center.IsHalfBlock)
        {
            return (mask, paintType, fullBright);
        }

        var upLeft = Framing.GetTileSafely(i - 1, j - 1);
        if (center.Slope.UpLeft && upLeft.Slope.DownRight
         && (edgeMask & 1) == 0 && (edgeMask & 4) == 0)
        {
            Check(upLeft, 1);
        }

        var upRight = Framing.GetTileSafely(i + 1, j - 1);
        if (center.Slope.UpRight && upRight.Slope.DownLeft
         && (edgeMask & 1) == 0 && (edgeMask & 8) == 0)
        {
            Check(upRight, 4);
        }

        return (mask, paintType, fullBright);

        void Check(Tile tile, int bit)
        {
            if (tile.TileType != type || !TileDrawing.IsVisible(tile))
            {
                return;
            }

            mask |= bit;

            if (paintType == 0)
            {
                paintType = tile.TileColor;
            }

            fullBright = tile.IsTileFullbright;
        }
    }
#endregion

#region Paint
    private static readonly Dictionary<TileMergingVariantKey, TileMergingRenderTargetHolder> paint_cache = [];

    internal readonly record struct TileMergingVariantKey(int Type, int PaintColor);

    internal sealed class TileMergingRenderTargetHolder(TileMergingVariantKey key, Asset<Texture2D> asset, int copySettingsFrom = -1)
        : TilePaintSystemV2.ARenderTargetHolder
    {
        public TileMergingVariantKey Key = key;

        public TreePaintingSettings PaintSettings = TreePaintSystemData.GetTileSettings(copySettingsFrom, 0);

        public Asset<Texture2D> Texture = asset;

        public override void Prepare()
        {
            Texture.Wait();

            PrepareTextureIfNecessary(Texture.Value);
        }

        public override void PrepareShader()
        {
            PrepareShader(Key.PaintColor, PaintSettings);
        }
    }

    private static bool TryGetPaintTexture(
        int type,
        int paintColor,
        Asset<Texture2D> asset,
        [NotNullWhen(true)] out Texture2D? texture
    )
    {
        texture = null;

        var key = new TileMergingVariantKey(type, paintColor);

        if (paint_cache.TryGetValue(key, out var holder) &&
            holder.IsReady)
        {
            texture = holder.Target;

            return true;
        }

        var newHolder = new TileMergingRenderTargetHolder(key, asset);

        paint_cache[key] = newHolder;

        Main.instance.TilePaintSystem._requests.Add(newHolder);

        return false;
    }
#endregion
}
