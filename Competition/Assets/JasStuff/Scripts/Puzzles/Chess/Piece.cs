using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Piece : MonoBehaviour
{
    public enum PieceType
    {
        Pawn,
        Rook,
        Knight,
        Bishop,
        Queen,
        King
    }
    public PieceType pieceType;
    private bool canMove = true;
    public void SetMove(bool v) => canMove = v;
    [SerializeField] private bool isWhite;
    public void SetWhite(bool v) => isWhite = v;
    public bool IsWhite() => isWhite;
    private List<Vector3Int> currentValidMoves = new List<Vector3Int>();

    private Tilemap boardTileMap;
    private Tilemap highlightTileMap;
    private TileBase highlightTile;
    private bool isHighlighted = false;
    private Vector3 previousPosition;
    private PieceManager manager;

    public void Init(Tilemap board, Tilemap highlight, TileBase tile, PieceManager mgr)
    {
        boardTileMap = board;
        highlightTileMap = highlight;
        highlightTile = tile;
        manager = mgr;
    }

    public void ToggleHighlight()
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
    public List<Vector3Int> GetValidMoves(Vector3Int currCell)
    {
        List<Vector3Int> validMoves = new();

        bool IsBlocked(Vector3Int cell, out bool enemy)
        {
            enemy = false;
            foreach (var piece in FindObjectsOfType<Piece>())
            {
                Vector3Int pieceCell = boardTileMap.WorldToCell(piece.transform.position);
                if (pieceCell == cell)
                {
                    if (piece.isWhite != this.isWhite) enemy = true;
                    return true;
                }
            }
            return false;
        }

        void AddDirectionalMoves(Vector3Int direction)
        {
            for (int i = 1; i < 8; i++)
            {
                Vector3Int next = currCell + direction * i;
                if (next.x < 0 || next.x >= 8 || next.y < 0 || next.y >= 8)
                    break;

                bool enemy;
                if (IsBlocked(next, out enemy))
                {
                    if (enemy) validMoves.Add(next);
                    break;
                }

                validMoves.Add(next);
            }
        }

        switch (pieceType)
        {
            case PieceType.Pawn:
                int dir = isWhite ? 1 : -1;
                Vector3Int forward = currCell + new Vector3Int(0, dir, 0);

                if (!IsBlocked(forward, out _))
                {
                    validMoves.Add(forward);

                    // first double move
                    if ((isWhite && currCell.y == 1) || (!isWhite && currCell.y == 6))
                    {
                        Vector3Int doubleMove = currCell + new Vector3Int(0, dir * 2, 0);
                        if (!IsBlocked(doubleMove, out _))
                            validMoves.Add(doubleMove);
                    }
                }

                Vector3Int[] captures = {
                currCell + new Vector3Int(1, dir, 0),
                currCell + new Vector3Int(-1, dir, 0)
                };

                foreach (var diag in captures)
                    if (IsBlocked(diag, out bool enemy) && enemy)
                        validMoves.Add(diag);
                break;

            case PieceType.Rook:
                AddDirectionalMoves(Vector3Int.right);
                AddDirectionalMoves(Vector3Int.left);
                AddDirectionalMoves(Vector3Int.up);
                AddDirectionalMoves(Vector3Int.down);
                break;

            case PieceType.Bishop:
                AddDirectionalMoves(Vector3Int.up + Vector3Int.right);
                AddDirectionalMoves(Vector3Int.up + Vector3Int.left);
                AddDirectionalMoves(Vector3Int.down + Vector3Int.right);
                AddDirectionalMoves(Vector3Int.down + Vector3Int.left);
                break;

            case PieceType.Queen:
                AddDirectionalMoves(Vector3Int.right);
                AddDirectionalMoves(Vector3Int.left);
                AddDirectionalMoves(Vector3Int.up);
                AddDirectionalMoves(Vector3Int.down);
                AddDirectionalMoves(Vector3Int.up + Vector3Int.right);
                AddDirectionalMoves(Vector3Int.up + Vector3Int.left);
                AddDirectionalMoves(Vector3Int.down + Vector3Int.right);
                AddDirectionalMoves(Vector3Int.down + Vector3Int.left);
                break;

            case PieceType.Knight:
                validMoves.AddRange(new[]
                {
                currCell + new Vector3Int(1, 2, 0),
                currCell + new Vector3Int(2, 1, 0),
                currCell + new Vector3Int(-1, 2, 0),
                currCell + new Vector3Int(-2, 1, 0),
                currCell + new Vector3Int(1, -2, 0),
                currCell + new Vector3Int(2, -1, 0),
                currCell + new Vector3Int(-1, -2, 0),
                currCell + new Vector3Int(-2, -1, 0)
            });
                break;

            case PieceType.King:
                for (int x = -1; x <= 1; x++)
                    for (int y = -1; y <= 1; y++)
                        if (!(x == 0 && y == 0))
                        {
                            Vector3Int next = currCell + new Vector3Int(x, y, 0);
                            if (!IsBlocked(next, out bool enemy) || enemy)
                                validMoves.Add(next);
                        }
                break;
        }

        return validMoves;
    }
    public bool TryMoveTo(Vector3 worldPos)
    {
        if (!canMove) return false;

        Vector3Int cell = boardTileMap.WorldToCell(worldPos);
        if (cell.x < 0 || cell.x > 7 || cell.y < 0 || cell.y > 7) return false;
        if (!currentValidMoves.Contains(cell)) return false;

        foreach (var piece in FindObjectsOfType<Piece>())
        {
            Vector3Int pieceCell = boardTileMap.WorldToCell(piece.transform.position);
            if (pieceCell == cell)
            {
                if (piece == this || piece.isWhite == this.isWhite)
                    return false;

                Destroy(piece.gameObject);
                break;
            }
        }

        previousPosition = transform.position;
        transform.position = boardTileMap.GetCellCenterWorld(cell);

        highlightTileMap.ClearAllTiles();
        isHighlighted = false;

        Vector3Int fromCell = boardTileMap.WorldToCell(previousPosition);
        Vector3Int toCell = boardTileMap.WorldToCell(transform.position);
        manager.OnPieceMoved(this, fromCell, toCell);

        return true;
    }
}
