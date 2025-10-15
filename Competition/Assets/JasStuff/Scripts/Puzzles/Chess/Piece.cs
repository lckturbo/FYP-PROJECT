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
    [SerializeField] private bool isWhite;
    public void SetWhite(bool v) => isWhite = v; 

    private Tilemap boardTileMap;
    private Tilemap highlightTileMap;
    private TileBase highlightTile;

    public void Init(Tilemap board, Tilemap highlight, TileBase tile)
    {
        boardTileMap = board;
        highlightTileMap = highlight;
        highlightTile = tile;
    }
}
