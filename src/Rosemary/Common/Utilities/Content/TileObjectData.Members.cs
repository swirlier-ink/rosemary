using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;

namespace Rosemary.Common;

partial class TileObjectDataBuilder<T>
{
    /// <inheritdoc cref="TileObjectData.CopyFrom"/>
    public T CopyFrom(TileObjectData data)
    {
        Data.CopyFrom(data);
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.UsesCustomCanPlace"/>
    public T UsesCustomCanPlace(bool value)
    {
        Data.UsesCustomCanPlace = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.UsesGlobalLiquidChecks"/>
    public T UsesGlobalLiquidChecks(bool value)
    {
        Data.UsesGlobalLiquidChecks = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.AnchorTop"/>
    public T AnchorTop(AnchorData value)
    {
        Data.AnchorTop = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.AnchorBottom"/>
    public T AnchorBottom(AnchorData value)
    {
        Data.AnchorBottom = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.AnchorLeft"/>
    public T AnchorLeft(AnchorData value)
    {
        Data.AnchorLeft = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.AnchorRight"/>
    public T AnchorRight(AnchorData value)
    {
        Data.AnchorRight = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.AnchorWall"/>
    public T AnchorWall(bool value)
    {
        Data.AnchorWall = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.AnchorValidTiles"/>
    public T AnchorValidTiles(params int[] value)
    {
        Data.AnchorValidTiles = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.AnchorInvalidTiles"/>
    public T AnchorInvalidTiles(params int[] value)
    {
        Data.AnchorInvalidTiles = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.AnchorAlternateTiles"/>
    public T AnchorAlternateTiles(params int[] value)
    {
        Data.AnchorAlternateTiles = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.AnchorValidWalls"/>
    public T AnchorValidWalls(params int[] value)
    {
        Data.AnchorValidWalls = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.WaterDeath"/>
    public T WaterDeath(bool value)
    {
        Data.WaterDeath = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.LavaDeath"/>
    public T LavaDeath(bool value)
    {
        Data.LavaDeath = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.WaterPlacement"/>
    public T WaterPlacement(LiquidPlacement value)
    {
        Data.WaterPlacement = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.LavaPlacement"/>
    public T LavaPlacement(LiquidPlacement value)
    {
        Data.LavaPlacement = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.HookCheckIfCanPlace"/>
    public T HookCheckIfCanPlace(PlacementHook value)
    {
        Data.HookCheckIfCanPlace = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.HookPostPlaceEveryone"/>
    public T HookPostPlaceEveryone(PlacementHook value)
    {
        Data.HookPostPlaceEveryone = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.HookPostPlaceMyPlayer"/>
    public T HookPostPlaceMyPlayer(PlacementHook value)
    {
        Data.HookPostPlaceMyPlayer = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.HookPlaceOverride"/>
    public T HookPlaceOverride(PlacementHook value)
    {
        Data.HookPlaceOverride = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.GetStyleOverride"/>
    public T GetStyleOverride(GetStyleMethod value)
    {
        Data.GetStyleOverride = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.DrawYOffset"/>
    public T DrawYOffset(int value)
    {
        Data.DrawYOffset = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.DrawXOffset"/>
    public T DrawXOffset(int value)
    {
        Data.DrawXOffset = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.DrawFlipHorizontal"/>
    public T DrawFlipHorizontal(bool value)
    {
        Data.DrawFlipHorizontal = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.DrawFlipVertical"/>
    public T DrawFlipVertical(bool value)
    {
        Data.DrawFlipVertical = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.DrawStepDown"/>
    public T DrawStepDown(int value)
    {
        Data.DrawStepDown = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.StyleHorizontal"/>
    public T StyleHorizontal(bool value)
    {
        Data.StyleHorizontal = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.Style"/>
    public T Style(int value)
    {
        Data.Style = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.StyleWrapLimit"/>
    public T StyleWrapLimit(int value)
    {
        Data.StyleWrapLimit = value;
        return (T)this;
    }

    // StyleWrapLimitVisualOverride

    // styleLineSkipVisualOverride

    /// <inheritdoc cref="TileObjectData.StyleLineSkip"/>
    public T StyleLineSkip(int value)
    {
        Data.StyleLineSkip = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.StyleMultiplier"/>
    public T StyleMultiplier(int value)
    {
        Data.StyleMultiplier = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.Width"/>
    public T Width(int value)
    {
        Data.Width = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.Height"/>
    public T Height(int value)
    {
        Data.Height = value;
        return (T)this;
    }

    /// <summary>
    ///     The size of this multi-tile in tile coordinates.<br/><br/>
    ///     Read the <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Tile#width">Width and Height section of the Basic Tile Guide on the wiki</see>
    ///     for more information and helpful visualizations.
    /// </summary>
    public T Size(Point value)
    {
        Data.Width = value.X;
        Data.Height = value.Y;
        return (T)this;
    }

    /// <inheritdoc cref="Size(Point)"/>
    public T Size(Point16 value)
    {
        Data.Width = value.X;
        Data.Height = value.Y;
        return (T)this;
    }

    /// <inheritdoc cref="Size(Point)"/>
    public T Size(int width, int height)
    {
        Data.Width = width;
        Data.Height = height;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.Origin"/>
    public T Origin(Point value)
    {
        Data.Origin = new Point16(value);
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.Origin"/>
    public T Origin(Point16 value)
    {
        Data.Origin = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.Origin"/>
    public T Origin(int x, int y)
    {
        Data.Origin = new Point16(x, y);
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.Direction"/>
    public T Direction(TileObjectDirection value)
    {
        Data.Direction = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.RandomStyleRange"/>
    public T RandomStyleRange(int value)
    {
        Data.RandomStyleRange = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.SpecificRandomStyles"/>
    public T SpecificRandomStyles(params int[] value)
    {
        Data.SpecificRandomStyles = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.FlattenAnchors"/>
    public T FlattenAnchors(bool value)
    {
        Data.FlattenAnchors = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.CoordinateHeights"/>
    public T CoordinateHeights(params int[] value)
    {
        Data.CoordinateHeights = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.DrawFrameOffsets"/>
    public T DrawFrameOffsets(Rectangle[,] value)
    {
        Data.DrawFrameOffsets = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.CoordinateWidth"/>
    public T CoordinateWidth(int value)
    {
        Data.CoordinateWidth = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.CoordinatePadding"/>
    public T CoordinatePadding(int value)
    {
        Data.CoordinatePadding = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.CoordinatePaddingFix"/>
    public T CoordinatePaddingFix(Point16 value)
    {
        Data.CoordinatePaddingFix = value;
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.CoordinatePaddingFix"/>
    public T CoordinatePaddingFix(Point value)
    {
        Data.CoordinatePaddingFix = new Point16(value);
        return (T)this;
    }

    /// <inheritdoc cref="TileObjectData.DrawStyleOffset"/>
    public T DrawStyleOffset(int value)
    {
        Data.DrawStyleOffset = value;
        return (T)this;
    }
}

partial class PrimaryTileObjectDataBuilder
{
    /// <inheritdoc cref="TileObjectData.FullCopyFrom(ushort)"/>
    public PrimaryTileObjectDataBuilder FullCopyFrom(int tileType)
    {
        Data.FullCopyFrom(TileObjectData.GetTileData(tileType, 0));
        return this;
    }

    /// <inheritdoc cref="TileObjectData.FullCopyFrom(TileObjectData)"/>
    public PrimaryTileObjectDataBuilder FullCopyFrom(TileObjectData data)
    {
        Data.FullCopyFrom(data);
        return this;
    }
}

partial class AlternateTileObjectDataBuilder;

partial class SubTileObjectDataBuilder
{
    /// <inheritdoc cref="TileObjectData.LinkedAlternates"/>
    public SubTileObjectDataBuilder LinkedAlternates(bool value)
    {
        Data.LinkedAlternates = value;
        return this;
    }
}
