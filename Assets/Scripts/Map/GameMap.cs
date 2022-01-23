using System.Collections.Generic;
using SardineFish.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Duality
{
    [RequireComponent(typeof(Tilemap))]
    public class GameMap : RuntimeSingleton<GameMap>, ICustomEditorEX
    {
        
        private Tilemap _tilemap;
        [SerializeField] private Tilemap _collisionTile;
        [SerializeField] private Tilemap _whiteCollisionTile;
        [SerializeField] private TileBase _backTile;
        [SerializeField] private TileBase _whiteTile;

        protected override void Awake()
        {
            base.Awake();
            _tilemap = GetComponent<Tilemap>();

        }

        [EditorButton]
        void GenCollider()
        {
            _whiteCollisionTile.ClearAllTiles();
            _collisionTile.ClearAllTiles();
            _tilemap = GetComponent<Tilemap>();
            var list = new List<Vector2Int>();
            list.Add(Vector2Int.zero);

            void AddPoint(Vector2Int point)
            {
                if (list.Contains(point))
                    return;
                list.Add(point);
            }
            for (var i = 0; i < list.Count; ++i)
            {
                var pos = list[i].ToVector3Int();
                var tile = _tilemap.GetTile(pos);
                if(!tile)
                    continue;
                
                if (tile == _whiteTile)
                {
                    _whiteCollisionTile.SetTile(pos, _whiteTile);
                }
                else if (tile == _backTile)
                {
                    _collisionTile.SetTile(pos, _backTile);
                }

                AddPoint(list[i] + Vector2Int.up);
                AddPoint(list[i] + Vector2Int.down);
                AddPoint(list[i] + Vector2Int.left);
                AddPoint(list[i] + Vector2Int.right);
            }
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
            if(newTile == _backTile)
                _collisionTile.SetTile(pos.ToVector3Int(), newTile);
            else
                _collisionTile.SetTile(pos.ToVector3Int(), null);
            // DualityMap._tilemap.SetTile(pos.ToVector3Int(), null);
            return tile;
        }
    }
}