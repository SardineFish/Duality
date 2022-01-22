using SardineFish.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Duality
{
    [RequireComponent(typeof(Tilemap))]
    public class GameMap : RuntimeSingleton<GameMap>
    {
        
        private Tilemap _tilemap;
        protected void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
        }

        public TileBase GetTileAt(Vector2 pos)
        {
            return _tilemap.GetTile(pos.ToVector3Int());
        }

        public TileBase RemoveTileAt(Vector2 pos)
        {
            var tile = GetTileAt(pos);
            _tilemap.SetTile(pos.ToVector3Int(), null);
            // DualityMap._tilemap.SetTile(pos.ToVector3Int(), DualityMap.GroundBlock);
            return tile;
        }

        public TileBase SetTileAt(Vector2 pos, TileBase newTile)
        {
            var tile = GetTileAt(pos);
            _tilemap.SetTile(pos.ToVector3Int(), newTile);
            // DualityMap._tilemap.SetTile(pos.ToVector3Int(), null);
            return tile;
        }
    }
}