using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace Rosemary.Common;

public static class TileExtensions
{
    public static bool ShowInvisibleTiles => Main.instance.TilesRenderer._shouldShowInvisibleBlocks;

    extension(Tile tile)
    {

        public bool HasSolidTile => tile.HasTile && Main.tileBlockLight[tile.type] && Main.tileSolid[tile.type] && tile.Slope.Block && (!tile.IsTileInvisible || ShowInvisibleTiles);

        public bool HasSolidWall => tile.HasWall && !Main.wallLight[tile.WallType] && !WallID.Sets.Transparent[tile.WallType] && (!tile.IsWallInvisible || ShowInvisibleTiles);

        public bool IsAir => !tile.HasTile && !tile.HasWall;

        public bool BlocksLight => !tile.IsAir && (tile.HasSolidTile || tile.HasSolidWall);

        public bool HasWall => tile.WallType != WallID.None;

        public bool TileCoatedOrPainted => tile.TilePainted || tile.TileCoated;

        public bool WallCoatedOrPainted => tile.WallPainted || tile.WallCoated;

        public bool TilePainted => tile.TileColor > PaintID.None;

        public bool WallPainted => tile.WallColor > PaintID.None;

        public bool TileCoated => tile.IsTileInvisible || tile.IsTileFullbright;

        public bool WallCoated => tile.IsWallInvisible || tile.IsWallFullbright;
    }

    extension(SlopeType slope)
    {
        public bool Block => slope == SlopeType.Solid;

        public bool Up => slope.Block || slope is SlopeType.SlopeUpLeft or SlopeType.SlopeUpRight;

        public bool Down => slope.Block || slope is SlopeType.SlopeDownLeft or SlopeType.SlopeDownRight;

        public bool Left => slope.Block || slope is SlopeType.SlopeUpLeft or SlopeType.SlopeDownLeft;

        public bool Right => slope.Block || slope is SlopeType.SlopeUpRight or SlopeType.SlopeDownRight;

        public bool UpLeft => slope.Up || slope.Left;

        public bool DownLeft => slope.Down || slope.Left;

        public bool UpRight => slope.Up || slope.Right;

        public bool DownRight => slope.Down || slope.Right;
    }
}

public static class TileDrawingExtensions
{
    extension(TileDrawing)
    {
        public static TileDrawing Instance => Main.instance.TilesRenderer;
    }
}

public static class WallDrawingExtensions
{
    extension(WallDrawing)
    {
        public static WallDrawing Instance => Main.instance.WallsRenderer;
    }
}

