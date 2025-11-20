using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PieceManager : BoardManager, IDataPersistence
{
    public static PieceManager instance;
    [Header("Reference")]
    [SerializeField] private GameObject piecePrefab;

    [Header("Sprites")]
    [SerializeField] private Sprite[] whitePieceSprites;
    [SerializeField] private Sprite[] blackPieceSprites;

    [Header("Puzzle Setup")]
    [SerializeField] private PuzzleDatabase puzzleDB;
    private int currentStep = 0;
    private int currentPuzzleIndex = 0;
    public int CurrentPuzzleIndex => currentPuzzleIndex;

    private PuzzleSolution activeSolution;
    private int activeSolutionIndex = -1; 

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(gameObject);
    }
    private void SpawnPiece(Piece.PieceType type, bool isWhite, Sprite sprite, Vector3Int cellPos)
    {
        GameObject spawned = SpawnOnBoard(piecePrefab, cellPos);

        var renderer = spawned.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;

        var p = spawned.GetComponent<Piece>();
        p.pieceType = type;
        p.SetWhite(isWhite);
        p.Init(boardTileMap, highlightTileMap, highlightTile, this);
        var glow = spawned.GetComponent<PieceGlow>();
        if (glow)
            glow.isWhite = isWhite;

        if (!spawnedObjs.Contains(spawned))
            spawnedObjs.Add(spawned);
    }

    protected override void SetupPuzzle()
    {
        ClearBoard();

        var currentPuzzle = puzzleDB.puzzles[currentPuzzleIndex];

        if (activeSolutionIndex < 0 || activeSolutionIndex >= currentPuzzle.solutions.Count)
        {
            activeSolutionIndex = Random.Range(0, currentPuzzle.solutions.Count);
            Debug.Log($"Selected NEW solution index: {activeSolutionIndex}");
        }
        else
        {
            Debug.Log($"Using SAVED solution index: {activeSolutionIndex}");
        }

        activeSolution = currentPuzzle.solutions[activeSolutionIndex];

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

    protected override void LockAllPieces()
    {
        foreach (var obj in spawnedObjs)
        {
            var p = obj.GetComponent<Piece>();
            if (p) p.SetMove(false);
        }
    }
    public override void OnMove(GameObject moved, Vector3Int fromCell, Vector3Int toCell)
    {
        var p = moved.GetComponent<Piece>();
        if (p == null) return;

        var expectedMove = activeSolution.moves[currentStep];
        bool correctMove = (expectedMove.type == p.pieceType && expectedMove.isWhite == p.IsWhite() && expectedMove.targetCell == toCell);

        AudioManager.instance.PlaySFXAtPoint("bottleput-88659", transform.position);

        if (correctMove)
        {
            Debug.Log($"Correct move ({currentStep + 1}/{activeSolution.moves.Count})");
            currentStep++;

            if (SaveLoadSystem.instance != null)
            {
                SaveLoadSystem.instance.SaveGame();
            }

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
                puzzleSolved = true;
                LockAllPieces();
                StartCoroutine(HandlePuzzleComplete());
            }
        }
        else
        {
            Debug.Log("Wrong move! Resetting...");
            StartCoroutine(ResetAfterDelay(1.2f));
        }
    }

    private IEnumerator DoBlackMove(PuzzleMove move)
    {
        yield return new WaitForSeconds(1f);

        var piece = spawnedObjs
          .Select(o => o.GetComponent<Piece>())
          .Where(p => p != null)
          .FirstOrDefault(p =>
              !p.IsWhite() &&
              p.pieceType == move.type &&
              boardTileMap.WorldToCell(p.transform.position) == move.startCell);

        if (piece != null)
        {
            Vector3Int targetCell = move.targetCell;

            var capturedPiece = spawnedObjs
             .Select(o => o.GetComponent<Piece>())
             .Where(p => p != null && p.IsWhite())
             .FirstOrDefault(p => boardTileMap.WorldToCell(p.transform.position) == targetCell);

            if (capturedPiece != null)
            {
                Destroy(capturedPiece.gameObject);
                spawnedObjs.Remove(capturedPiece.gameObject);
            }

            Vector3 worldTarget = boardTileMap.GetCellCenterWorld(targetCell);
            worldTarget.z = piece.transform.position.z;
            piece.transform.position = worldTarget;
            currentStep++;

            if (SaveLoadSystem.instance != null)
            {
                SaveLoadSystem.instance.SaveGame();
            }
        }
    }
    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetBoard();
        currentStep = 0;

        if (SaveLoadSystem.instance != null)
        {
            SaveLoadSystem.instance.SaveGame();
        }
    }

    public void LoadData(GameData data)
    {
        StartCoroutine(LoadChessPiecesRoutine(data));
    }

    private IEnumerator LoadChessPiecesRoutine(GameData data)
    {
        yield return null;

        ClearBoard();

        if (data.chessPuzzleSolutionIndex >= 0)
        {
            activeSolutionIndex = data.chessPuzzleSolutionIndex;
            currentStep = data.chessPuzzleCurrentStep;
        }

        if (data.chessPieces != null && data.chessPieces.Count > 0)
        {
            var currentPuzzle = puzzleDB.puzzles[currentPuzzleIndex];
            if (activeSolutionIndex >= 0 && activeSolutionIndex < currentPuzzle.solutions.Count)
            {
                activeSolution = currentPuzzle.solutions[activeSolutionIndex];
            }

            foreach (var pieceData in data.chessPieces)
            {
                Sprite sprite = pieceData.isWhite
                    ? whitePieceSprites[(int)pieceData.type]
                    : blackPieceSprites[(int)pieceData.type];

                SpawnPiece(pieceData.type, pieceData.isWhite, sprite, pieceData.position);
            }

            if (data.chessPuzzleCompleted && doorToOpen != null)
            {
                puzzleSolved = true;
                doorToOpen.Open();
            }
            else
                puzzleSolved = false;
        }
        else
        {
            puzzleSolved = false;
            activeSolutionIndex = -1; 
            currentStep = 0;
            SetupPuzzle();
        }
    }

    public void SaveData(ref GameData data)
    {
        data.chessPuzzleSolutionIndex = activeSolutionIndex;
        data.chessPuzzleCurrentStep = currentStep;

        if (!puzzleSolved)
        {
            data.chessPuzzleCompleted = false;
            // Continue saving piece positions below
        }

        if (data.chessPieces == null)
            data.chessPieces = new List<GameData.ChessPieceSaveData>();
        data.chessPieces.Clear();

        foreach (var obj in spawnedObjs)
        {
            var piece = obj.GetComponent<Piece>();
            if (piece != null)
            {
                Vector3Int cellPos = boardTileMap.WorldToCell(piece.transform.position);
                data.chessPieces.Add(new GameData.ChessPieceSaveData(
                    piece.pieceType,
                    piece.IsWhite(),
                    cellPos
                ));
            }
        }

        if (puzzleSolved)
        {
            data.chessPuzzleCompleted = true;
        }
    }

    public PuzzleSolution GetActiveSolution()
    {
        return activeSolution;
    }
}