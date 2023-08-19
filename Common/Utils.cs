using Terraria;
using Terraria.DataStructures;
using Terraria.ObjectData;

namespace SyntheticEvolution.Common;

public static class Utils
{
    public static Point16 GetTopLeftTileInMultitile(int x, int y)
    {
        Tile tile = Main.tile[x, y];

        int frameX = 0;
        int frameY = 0;

        if (tile.HasTile)
        {
            int style = 0, alt = 0;
            TileObjectData.GetTileInfo(tile, ref style, ref alt);
            TileObjectData data = TileObjectData.GetTileData(tile.TileType, style, alt);

            if (data != null)
            {
                int size = 16 + data.CoordinatePadding;

                frameX = tile.TileFrameX % (size * data.Width) / size;
                frameY = tile.TileFrameY % (size * data.Height) / size;
            }
        }

        return new Point16(x - frameX, y - frameY);
    }
    
    public static bool TryGetTileEntityAs<T>(int i, int j, out T entity) where T : TileEntity
    {
        Point16 origin = GetTopLeftTileInMultitile(i, j);

        if (TileEntity.ByPosition.TryGetValue(origin, out TileEntity existing) && existing is T existingAsT)
        {
            entity = existingAsT;
            return true;
        }

        entity = null;
        return false;
    }
}