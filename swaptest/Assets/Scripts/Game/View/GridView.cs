using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.View
{
    public class GridView: MonoBehaviour
    {
        [SerializeField] Tilemap _tilemap;
        [SerializeField] TileBase _tile;

        public void Init(int rows, int cols, Vector3 offset)
        {
            _tilemap.ClearAllTiles();
            for(int i = 0; i < rows; ++i)
            {
                for(int j = 0; j < cols; ++j)
                {
                    _tilemap.SetTile(new Vector3Int(i, j, 0), _tile);
                }
            }
            transform.localPosition = offset;
        }
    }
}
