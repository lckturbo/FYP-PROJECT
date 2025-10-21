using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StatueManager : BoardManager
{
    [Header("References")]
    [SerializeField] private GameObject whiteStatuePrefab;
    [SerializeField] private GameObject blackStatuePrefab;

    private bool whiteTurn;

    private Vector3Int[] whitePositions;
    private Vector3Int[] blackPositions;

    private static Vector3Int[][] WinningLines = new Vector3Int[][]
  {
    new Vector3Int[] { new(0,3,0), new(3,3,0), new(6,3,0) }, // Horizontal
    new Vector3Int[] { new(3,6,0), new(3,3,0), new(3,0,0) }, // Vertical
    new Vector3Int[] { new(1,1,0), new(3,3,0), new(5,5,0) }, // Diagonal \
    new Vector3Int[] { new(5,1,0), new(3,3,0), new(1,5,0) }  // Diagonal /
  };

    protected override void SetupPuzzle()
    {
        var ticTacToeBoard = FindObjectOfType<TicTacToeBoard>();
        if (ticTacToeBoard == null) return;

        var availableCells = new List<Vector3Int>(ticTacToeBoard.GetAllCells());
        availableCells.Remove(new Vector3Int(3, 3, 0));

        bool validSetup = false;
        int attempts = 0;

        while (!validSetup && attempts < 50)
        {
            attempts++;
            availableCells = availableCells.OrderBy(_ => Random.value).ToList();

            whitePositions = availableCells.Take(3).ToArray();
            blackPositions = availableCells.Skip(3).Take(3).ToArray();

            if (!HasLine(whitePositions.ToList()) && !HasLine(blackPositions.ToList()))
                validSetup = true;
        }

        foreach (var pos in whitePositions)
        {
            GameObject statue = SpawnOnBoard(whiteStatuePrefab, pos);
            var s = statue.GetComponent<Statue>();
            s.SetWhite(true);
            spawnedObjs.Add(statue);
            s.Init(boardTileMap, highlightTileMap, highlightTile, this);
        }

        foreach (var pos in blackPositions)
        {
            GameObject statue = SpawnOnBoard(blackStatuePrefab, pos);
            var s = statue.GetComponent<Statue>();
            s.SetWhite(false);
            spawnedObjs.Add(statue);
            s.Init(boardTileMap, highlightTileMap, highlightTile, this);
        }
        whiteTurn = true;
    }


    public override void OnMove(GameObject moved, Vector3Int fromCell, Vector3Int toCell)
    {
        if (puzzleSolved) return;
        CheckForLineWin();

        if (!puzzleSolved && whiteTurn)
        {
            whiteTurn = false;
            Invoke(nameof(BlackMove), 0.6f);
        }
        else
            whiteTurn = true;
    }

    private void CheckForLineWin()
    {
        List<Vector3Int> whiteCells = new();
        List<Vector3Int> blackCells = new();

        foreach (var obj in spawnedObjs)
        {
            var s = obj.GetComponent<Statue>();
            if (!s) continue;

            Debug.Log($"{(s.IsWhite() ? "White" : "Black")} at cell {s.CurrentCell}");

            if (s.IsWhite())
                whiteCells.Add(s.CurrentCell);
            else
                blackCells.Add(s.CurrentCell);
        }

        if (HasLine(whiteCells)) 
            DeclareWinner("White");
        else if (HasLine(blackCells)) 
            DeclareWinner("Black");
    }

    private void DeclareWinner(string winner)
    {
        Debug.Log($"{winner} Wins!");
        puzzleSolved = true;
        LockAllPieces();
    }
    private bool HasLine(List<Vector3Int> cells)
    {
        foreach (var line in WinningLines)
        {
            bool allMatch = true;

            foreach (var pos in line)
            {
                if (!cells.Contains(pos))
                {
                    allMatch = false;
                    break;
                }
            }

            if (allMatch)
            {
                Debug.Log($"Line found: {line[0]}, {line[1]}, {line[2]}");
                return true;
            }
        }

        return false;
    }
    protected override void LockAllPieces()
    {
        foreach (var obj in spawnedObjs)
        {
            var s = obj.GetComponent<Statue>();
            if (s) s.SetMove(false);
        }
    }

    private void BlackMove()
    {
        if (puzzleSolved) return;

        var blacks = spawnedObjs
            .Select(o => o.GetComponent<Statue>())
            .Where(s => s != null && !s.IsWhite())
            .OrderBy(_ => Random.value)
            .ToList();

        Statue bestStatue = null;
        Vector3Int bestMove = Vector3Int.zero;
        int bestScore = -1;

        foreach (var s in blacks)
        {
            var moves = s.GetValidMoves(s.CurrentCell);
            foreach (var move in moves)
            {
                int score = EvaluateMove(move, blacks.Select(b => b.CurrentCell).ToList());
                if (score > bestScore)
                {
                    bestScore = score;
                    bestStatue = s;
                    bestMove = move;
                }
            }
        }

        if (bestStatue == null) return;

        Debug.Log($"AI moves statue at {bestStatue.CurrentCell} to {bestMove}");
        StartCoroutine(DoBlackMove(bestStatue, bestMove));
    }


    private int EvaluateMove(Vector3Int move, List<Vector3Int> currentBlackCells)
    {
        List<Vector3Int> hypothetical = new(currentBlackCells) { move };
        int score = 0;

        foreach (var line in WinningLines)
        {
            int count = line.Count(p => hypothetical.Contains(p));
            score = Mathf.Max(score, count);
        }

        return score;
    }

    private IEnumerator DoBlackMove(Statue statue, Vector3Int targetCell)
    {
        yield return new WaitForSeconds(0.5f);

        var occupied = spawnedObjs
            .FirstOrDefault(o => boardTileMap.WorldToCell(o.transform.position) == targetCell);

        if (occupied != null)
        {
            Destroy(occupied);
            spawnedObjs.Remove(occupied);
        }

        Vector3 worldTarget = boardTileMap.GetCellCenterWorld(targetCell);
        worldTarget.z = statue.transform.position.z;
        statue.transform.position = worldTarget;
        statue.UpdateCell(targetCell);
        highlightTileMap.ClearAllTiles();
        Debug.Log($"AI moved existing statue to {targetCell}");

        OnMove(statue.gameObject, Vector3Int.zero, targetCell);
    }

}
