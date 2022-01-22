using SardineFish.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Duality
{
    [RequireComponent(typeof(Tilemap))]
    public class GameMap : RuntimeSingleton<GameMap>
    {
        private Tilemap _tilemap;
        protected override void Awake()
        {
            base.Awake();


            _tilemap = GetComponent<Tilemap>();
        }

        public TileBase GetTileAt(Vector2 pos)
        {
            return _tilemap.GetTile(pos.ToVector3Int());
        }
    }
}