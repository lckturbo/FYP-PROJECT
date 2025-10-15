using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PieceManager : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Tilemap boardTileMap;
    [SerializeField] private GameObject piecePrefab;

    [Header("Highlight Tile")]
    [SerializeField] private Tilemap highlightTileMap;
    [SerializeField] private TileBase highlightTile;

    [SerializeField] private Sprite[] whitePieceSprites;
    [SerializeField] private Sprite[] blackPieceSprites;

    private GameObject[,] pieceObjects = new GameObject[8, 8];

    private void Start()
    {
        SetupPuzzle1();
    }
    private void SpawnPiece(Piece.PieceType type, bool isWhite, Sprite sprite, Vector3Int cellPos)
    {
        Vector3 worldPos = boardTileMap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
        GameObject piece = Instantiate(piecePrefab, worldPos, Quaternion.identity, transform);

        // Set visuals
        piece.GetComponent<SpriteRenderer>().sprite = sprite;
        pieceObjects[cellPos.x, cellPos.y] = piece;

        // Configure logic
        var p = piece.GetComponent<Piece>();
        p.pieceType = type;
        p.SetWhite(isWhite);
        p.Init(boardTileMap, highlightTileMap, highlightTile);
    }

    private void SetupPuzzle1()
    {
        SpawnPiece(Piece.PieceType.King, true, whitePieceSprites[5], new Vector3Int(6, 4, 0)); // King g5
        SpawnPiece(Piece.PieceType.Queen, true, whitePieceSprites[4], new Vector3Int(6, 2, 0)); // Queen g3
        SpawnPiece(Piece.PieceType.Rook, true, whitePieceSprites[1], new Vector3Int(4, 1, 0)); // Rook e2
        SpawnPiece(Piece.PieceType.Bishop, true, whitePieceSprites[3], new Vector3Int(7, 0, 0)); // Bishop h1
        SpawnPiece(Piece.PieceType.Knight, true, whitePieceSprites[2], new Vector3Int(4, 6, 0)); // Knight e7

        SpawnPiece(Piece.PieceType.King, false, blackPieceSprites[5], new Vector3Int(3, 3, 0)); // King d4
        SpawnPiece(Piece.PieceType.Knight, true, blackPieceSprites[2], new Vector3Int(2, 3, 0)); // Knight c4
        SpawnPiece(Piece.PieceType.Knight, true, blackPieceSprites[2], new Vector3Int(1, 1, 0)); // Knight b2
        SpawnPiece(Piece.PieceType.Pawn, true, blackPieceSprites[0], new Vector3Int(1, 5, 0)); // Pawn b6
    }

    private void SetupPuzzle2()
    {

    }

    private void SetupPuzzle3()
    {
    }
}
