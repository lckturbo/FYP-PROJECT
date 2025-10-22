using UnityEngine;
using UnityEngine.Tilemaps;

public class TicTacToeBoard : BoardBase
{
    [SerializeField] private TileBase slotTile;
    [SerializeField] private TileBase centerTile;

    private Vector3Int[] circlePositions = new Vector3Int[]
    {
        new(1,1,0), // bottom left
        new(3,0,0), // bottom middle
        new(5,1,0), // bottom right

        new(0,3,0), // middle left
        new(3,3,0), // middle
        new(6,3,0), // middle right

        new(1,5,0), // top left
        new(3,6,0), // top middle
        new(5,5,0), // top right
    };
    public Vector3Int[] GetAllCells() => circlePositions;

    protected override void GenerateBoard()
    {
        foreach (var pos in circlePositions)
        {
            if (pos == new Vector3Int(3, 3, 0))
                boardTileMap.SetTile(pos, centerTile);
            else
                boardTileMap.SetTile(pos, slotTile);
        }
    }
}
