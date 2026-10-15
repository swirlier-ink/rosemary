using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Rosemary.Common;

public abstract class TileObjectDataBuilder
{
    public int Type { get; init; }

    public TileObjectData Data { get; init; }

    protected TileObjectDataBuilder(int type, TileObjectData data)
    {
        Type = type;
        Data = data;

        Data.UsesCustomCanPlace = true;
    }

    public TileObjectData Calculate()
    {
        Data.Calculate();

        return Data;
    }
}

public abstract partial class TileObjectDataBuilder<T>(int type, TileObjectData data)
    : TileObjectDataBuilder(type, data) where T : TileObjectDataBuilder<T>;

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

    public PrimaryTileObjectDataBuilder AddAlternateTile(int baseStyle, Func<TileObjectData, TileObjectData> func)
    {
        var alternate = func(Data);

        if (!Data._hasOwnAlternates)
        {
            Data.Alternates = [];
        }

        alternate.Style = baseStyle;
        alternate._parent = Data;

        Data.Alternates.Add(alternate);

        return this;
    }

    public PrimaryTileObjectDataBuilder AddSubTile(int startStyle, int styleCount, Func<TileObjectData, TileObjectData> func) =>
        AddSubTile(Enumerable.Range(startStyle, startStyle + styleCount).ToArray(), func);

    public PrimaryTileObjectDataBuilder AddSubTile(int[] styles, Func<TileObjectData, TileObjectData> func)
    {
        var subTile = func(Data);

        foreach (var num in styles)
        {
            List<TileObjectData?> list;

            if (!Data._hasOwnSubTiles)
            {
                list = new List<TileObjectData?>(num + 1);
                Data.SubTiles = list;
            }
            else
            {
                list = Data.SubTiles;
            }

            if (list.Count <= num)
            {
                for (var j = list.Count; j <= num; j++)
                {
                    list.Add(null);
                }
            }

            subTile._parent = Data;
            list[num] = subTile;
        }

        return this;
    }
}

public sealed partial class AlternateTileObjectDataBuilder(int type, TileObjectData data)
    : TileObjectDataBuilder<AlternateTileObjectDataBuilder>(type, data);

public sealed partial class SubTileObjectDataBuilder(int type, TileObjectData data)
    : TileObjectDataBuilder<SubTileObjectDataBuilder>(type, data);

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

        public static AlternateTileObjectDataBuilder NewAlternateTileData(TileObjectData? cloneStyle = null)
        {
            return new AlternateTileObjectDataBuilder(-1, cloneStyle ?? new TileObjectData(TileObjectData._baseObject));
        }

        public static SubTileObjectDataBuilder NewSubTileData(TileObjectData? cloneStyle = null)
        {
            return new SubTileObjectDataBuilder(-1, cloneStyle ?? new TileObjectData(TileObjectData._baseObject));
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
                   .Register()
                   .Data;

            return data;
        }
    }
}
