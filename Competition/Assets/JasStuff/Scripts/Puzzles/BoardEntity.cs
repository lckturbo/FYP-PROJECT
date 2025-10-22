using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class BoardEntity : MonoBehaviour
{
    protected Tilemap boardTileMap;
    protected Tilemap highlightTileMap;
    protected TileBase highlightTile;

    protected bool isWhite;
    protected bool isHighlighted = false;
    protected bool canMove = true;

    protected Vector3Int currentCell;
    public Vector3Int CurrentCell => currentCell;

    protected Vector3 previousPosition;
    protected List<Vector3Int> currentValidMoves = new List<Vector3Int>();

    public void SetMove(bool v) => canMove = v;
    public bool IsWhite() => isWhite;
    public void SetWhite(bool v) => isWhite = v;
    public virtual void Init(Tilemap board, Tilemap highlight, TileBase tile)
    {
        boardTileMap = board;
        highlightTileMap = highlight;
        highlightTile = tile;
        currentCell = board.WorldToCell(transform.position);
    }
    public virtual void ToggleHighlight()
    {
        if (!isWhite || !canMove) return;

        if (isHighlighted)
        {
            highlightTileMap.ClearAllTiles();
            isHighlighted = false;
        }
        else
        {
            HighlightMoves();
            isHighlighted = true;
        }
    }
    public void HighlightMoves()
    {
        highlightTileMap.ClearAllTiles();
        currentValidMoves.Clear();

        Vector3Int currCell = boardTileMap.WorldToCell(transform.position);
        List<Vector3Int> validMoves = GetValidMoves(currCell);

        foreach (var move in validMoves)
        {
            if (move.x >= 0 && move.x < 8 && move.y >= 0 && move.y < 8)
                highlightTileMap.SetTile(move, highlightTile);
        }

        currentValidMoves = validMoves;
    }

    public abstract List<Vector3Int> GetValidMoves(Vector3Int currCell);
    public abstract bool TryMoveTo(Vector3 worldPos);
    public void UpdateCell(Vector3Int newCell)
    {
        currentCell = newCell;
    }
}
