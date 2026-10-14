using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Rosemary.Common;

public abstract class TileObjectDataBuilder(int type, TileObjectData data, TileObjectDataBuilder? parent = null)
{
    protected TileObjectDataBuilder? Parent { get; init; } = parent;

    public int Type { get; init; } = type;

    public TileObjectData Data { get; init; } = data;
}

public abstract partial class TileObjectDataBuilder<T>(int type, TileObjectData data, TileObjectDataBuilder? parent = null)
    : TileObjectDataBuilder(type, data, parent) where T : TileObjectDataBuilder<T>;

public sealed partial class PrimaryTileObjectDataBuilder(int type, TileObjectData data)
    : TileObjectDataBuilder<PrimaryTileObjectDataBuilder>(type, data)
{
    public PrimaryTileObjectDataBuilder Register()
    {
        Data.Calculate();

        if (Type != -1)
        {
            TileObjectData._data[Type] = Data;
        }

        return this;
    }

    public AlternateTileObjectDataBuilder NewAlternateTile(int baseStyle, TileObjectData? cloneStyle = null)
    {
        return new AlternateTileObjectDataBuilder(Type, baseStyle, cloneStyle ?? new TileObjectData(TileObjectData._baseObject), this);
    }

    public SubTileObjectDataBuilder NewSubTile(int startStyle, int styleCount, TileObjectData? cloneStyle = null) =>
        NewSubTile(Enumerable.Range(startStyle, startStyle + styleCount).ToArray(), cloneStyle);

    public SubTileObjectDataBuilder NewSubTile(params int[] styles) =>
        NewSubTile(styles, null);

    public SubTileObjectDataBuilder NewSubTile(int[] styles, TileObjectData? cloneStyle)
    {
        return new SubTileObjectDataBuilder(Type, styles, cloneStyle ?? new TileObjectData(TileObjectData._baseObject), this);
    }
}

public sealed partial class AlternateTileObjectDataBuilder(int type, int baseStyle, TileObjectData data, TileObjectDataBuilder parent)
    : TileObjectDataBuilder(type, data, parent)
{
    public TileObjectDataBuilder Register()
    {
        if (Parent?.Data is not { } parentData)
        {
            return this;
        }

        Data.Calculate();

        if (!parentData._hasOwnAlternates)
        {
            parentData.Alternates = [];
        }

        Data.Style = baseStyle;
        Data._parent = parentData;

        parentData.Alternates.Add(Data);

        return Parent;
    }
}

public sealed partial class SubTileObjectDataBuilder(int type, int[] styles, TileObjectData data, TileObjectDataBuilder parent)
    : TileObjectDataBuilder(type, data, parent)
{
    public TileObjectDataBuilder Register()
    {
        if (Parent?.Data is not { } parentData)
        {
            return this;
        }

        Data.Calculate();

        foreach (var num in styles)
        {
            List<TileObjectData?> list;

            if (!parentData._hasOwnSubTiles)
            {
                list = new List<TileObjectData?>(num + 1);
                parentData.SubTiles = list;
            }
            else
            {
                list = parentData.SubTiles;
            }

            if (list.Count <= num)
            {
                for (var j = list.Count; j <= num; j++)
                {
                    list.Add(null);
                }
            }

            Data._parent = parentData;
            list[num] = Data;
        }

        return Parent;
    }
}

public static class TileObjectDataExtensions
{
    extension(ModTile tile)
    {
        public PrimaryTileObjectDataBuilder NewTileData(TileObjectData? cloneStyle = null)
        {
            return new PrimaryTileObjectDataBuilder(tile.Type, cloneStyle ?? new TileObjectData(TileObjectData._baseObject));
        }
    }

    extension(TileObjectData)
    {
        public static PrimaryTileObjectDataBuilder NewTileData(TileObjectData? cloneStyle = null) =>
            TileObjectData.NewTileData(-1, cloneStyle);

        public static PrimaryTileObjectDataBuilder NewTileData(int type, TileObjectData? cloneStyle = null)
        {
            return new PrimaryTileObjectDataBuilder(type, cloneStyle ?? new TileObjectData(TileObjectData._baseObject));
        }

        public static TileObjectData SizedStyle(int width, int height, Point origin, bool lastCoordinateHeight18 = false) =>
            TileObjectData.SizedStyle(new Point(width, height), origin, lastCoordinateHeight18);

        public static TileObjectData SizedStyle(Point size, Point origin, bool lastCoordinateHeight18 = false)
        {
            var heights = Enumerable.Repeat(16, size.Y).ToArray();

            if (lastCoordinateHeight18)
            {
                heights[^1] = 18;
            }

            return TileObjectData.SizedStyle(size, origin, heights);
        }

        public static TileObjectData SizedStyle(Point size, Point origin, params int[] coordinateHeights)
        {
            var data =
                TileObjectData
                   .NewTileData()
                   .Size(size)
                   .Origin(origin)
                   .AnchorBottom(new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table | AnchorType.SolidSide, size.X, 0))
                   .UsesCustomCanPlace(true)
                   .CoordinateHeights(coordinateHeights)
                   .CoordinateWidth(16)
                   .CoordinatePadding(2)
                   .LavaDeath(true) // Should this be default?
                   .Register()
                   .Data;

            return data;
        }
    }
}
