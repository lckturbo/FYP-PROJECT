using UnityEngine;
using UnityEngine.Tilemaps;

public class PieceBoard : BoardBase
{
    [SerializeField] private TileBase lightTile;
    [SerializeField] private TileBase darkTile;

    protected override void GenerateBoard()
    {
        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                var tile = (x + y) % 2 == 0 ? lightTile : darkTile;
                boardTileMap.SetTile(pos, tile);
            }
        }
    }
}
