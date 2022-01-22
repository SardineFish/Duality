using UnityEngine.Tilemaps;

namespace Duality
{
    public enum TileType
    {
        Black,
        White,
    }
    public class GameTile : Tile
    {
        public TileType Type;
    }
}