using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapBoard : MonoBehaviour
{
    [SerializeField] private Tilemap boardTileMap;
    [SerializeField] private TileBase lightTile;
    [SerializeField] private TileBase darkTile;

    private int boardSize = 8;

    private void Start()
    {
        DrawBoard();
    }
    void DrawBoard()
    {
        for(int x = 0; x < boardSize; x++)
        {
            for(int y = 0; y < boardSize; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                var tile = (x + y) % 2 == 0 ? lightTile : darkTile;
                boardTileMap.SetTile(pos, tile);
            }
        }
    }
}
