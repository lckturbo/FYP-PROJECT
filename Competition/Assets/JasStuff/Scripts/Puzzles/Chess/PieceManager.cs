using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PieceManager : MonoBehaviour
{
    [Header("Puzzle Setup")]
    [SerializeField] private PuzzleDatabase puzzleDB;
    private int currentStep = 0;
    private int currentPuzzleIndex = 0;
    private List<Piece> allPieces = new();

    [Header("Reference")]
    [SerializeField] private Tilemap boardTileMap;
    [SerializeField] private GameObject piecePrefab;

    [Header("Highlight Tile")]
    [SerializeField] private Tilemap highlightTileMap;
    [SerializeField] private TileBase highlightTile;

    [Header("Sprites")]
    [SerializeField] private Sprite[] whitePieceSprites;
    [SerializeField] private Sprite[] blackPieceSprites;
    private PuzzleSolution activeSolution;

    private GameObject[,] pieceObjects = new GameObject[8, 8];
    private bool puzzleSolved = false;

    private void Start()
    {
        SetupPuzzle();
    }

    private void SpawnPiece(Piece.PieceType type, bool isWhite, Sprite sprite, Vector3Int cellPos)
    {
        Vector3 worldPos = boardTileMap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
        GameObject piece = Instantiate(piecePrefab, worldPos, Quaternion.identity, transform);

        piece.GetComponent<SpriteRenderer>().sprite = sprite;
        pieceObjects[cellPos.x, cellPos.y] = piece;

        var p = piece.GetComponent<Piece>();
        p.pieceType = type;
        p.SetWhite(isWhite);
        p.Init(boardTileMap, highlightTileMap, highlightTile, this);

        allPieces.Add(p);
    }

    public void OnPieceMoved(Piece movedPiece, Vector3Int fromCell, Vector3Int toCell)
    {
        if (puzzleSolved || activeSolution == null) return;

        if (currentStep >= activeSolution.moves.Count) return;

        var expectedMove = activeSolution.moves[currentStep];
        bool correctMove = (
            expectedMove.type == movedPiece.pieceType &&
            expectedMove.isWhite == movedPiece.IsWhite() &&
            expectedMove.targetCell == toCell
        );

        if (correctMove)
        {
            Debug.Log($"Correct move ({currentStep + 1}/{activeSolution.moves.Count})");
            currentStep++;

            // AI turn next?
            if (currentStep < activeSolution.moves.Count)
            {
                var nextMove = activeSolution.moves[currentStep];
                if (!nextMove.isWhite)
                {
                    StartCoroutine(DoBlackMove(nextMove));
                }
            }
            else
            {
                Debug.Log("Puzzle solved!");
                puzzleSolved = true;
                LockAllPieces();
            }
        }
        else
        {
            Debug.Log("Wrong move! Resetting...");
            StartCoroutine(ResetAfterDelay(1.2f));
        }
    }


    private void LockAllPieces()
    {
        foreach (var piece in allPieces)
        {
            piece.SetMove(false);
        }
    }


    private IEnumerator DoBlackMove(PuzzleMove move)
    {
        yield return new WaitForSeconds(1f);

        var piece = allPieces.Find(p =>
            !p.IsWhite() &&
            p.pieceType == move.type &&
            boardTileMap.WorldToCell(p.transform.position) == move.startCell);

        if (piece != null)
        {
            Vector3Int targetCell = move.targetCell;

            var capturedPiece = allPieces.Find(p =>
                p.IsWhite() &&
                boardTileMap.WorldToCell(p.transform.position) == targetCell);

            if (capturedPiece != null)
            {
                Destroy(capturedPiece.gameObject);
                allPieces.Remove(capturedPiece);
            }

            Vector3 worldTarget = boardTileMap.GetCellCenterWorld(targetCell);
            worldTarget.z = piece.transform.position.z; 
            piece.transform.position = worldTarget;
            currentStep++;
        }
    }



    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetupPuzzle();
        currentStep = 0;
    }

    ////// PUZZLE SETUPS //////
    private void SetupPuzzle()
    {
        ClearBoard();

        var currentPuzzle = puzzleDB.puzzles[currentPuzzleIndex];
        activeSolution = currentPuzzle.solutions[Random.Range(0, currentPuzzle.solutions.Count)];
        Debug.Log($"Selected solution index: {currentPuzzle.solutions.IndexOf(activeSolution)}");
        // white pieces
        SpawnPiece(Piece.PieceType.King, true, whitePieceSprites[5], new Vector3Int(6, 4, 0)); // King g5
        SpawnPiece(Piece.PieceType.Queen, true, whitePieceSprites[4], new Vector3Int(6, 2, 0)); // Queen g3
        SpawnPiece(Piece.PieceType.Rook, true, whitePieceSprites[1], new Vector3Int(4, 1, 0)); // Rook e2
        SpawnPiece(Piece.PieceType.Bishop, true, whitePieceSprites[3], new Vector3Int(7, 0, 0)); // Bishop h1
        SpawnPiece(Piece.PieceType.Knight, true, whitePieceSprites[2], new Vector3Int(4, 6, 0)); // Knight e7

        // black pieces
        SpawnPiece(Piece.PieceType.King, false, blackPieceSprites[5], new Vector3Int(3, 3, 0)); // King d4
        SpawnPiece(Piece.PieceType.Knight, false, blackPieceSprites[2], new Vector3Int(2, 3, 0)); // Knight c4
        SpawnPiece(Piece.PieceType.Knight, false, blackPieceSprites[2], new Vector3Int(1, 1, 0)); // Knight b2
        SpawnPiece(Piece.PieceType.Pawn, false, blackPieceSprites[0], new Vector3Int(1, 5, 0)); // Pawn b6
    }

    private void ClearBoard()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        allPieces.Clear();
    }
}
