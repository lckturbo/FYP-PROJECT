using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StatueManager : BoardManager
{
    [Header("References")]
    [SerializeField] private GameObject whiteStatuePrefab;
    [SerializeField] private GameObject blackStatuePrefab;

    private readonly Vector3Int[] whitePositions =
   {
         new(3, 3, 0),
         new(6, 3, 0),
         new(3, 0, 0)
     };

    private readonly Vector3Int[] blackPositions =
    {
         new(5, 5, 0),
         new(0, 3, 0),
         new(5, 1, 0)
     };

    protected override void SetupPuzzle()
    {
        foreach (var pos in whitePositions)
        {
            GameObject statue = SpawnOnBoard(whiteStatuePrefab, pos);
            var s = statue.GetComponent<Statue>();
            s.SetWhite(true);
            s.Init(boardTileMap, highlightTileMap, highlightTile, this);
        }

        foreach (var pos in blackPositions)
        {
            GameObject statue = SpawnOnBoard(blackStatuePrefab, pos);
            var s = statue.GetComponent<Statue>();
            s.SetWhite(false);
            s.Init(boardTileMap, highlightTileMap, highlightTile, this);
        }
    }

    public override void OnMove(GameObject moved, Vector3Int fromCell, Vector3Int toCell)
    {
        CheckForLineWin();
    }

    private void CheckForLineWin()
    {
        //List<Vector3Int> whiteCells = new();
        //List<Vector3Int> blackCells = new();

        //foreach (var obj in spawnedObjs)
        //{
        //    var s = obj.GetComponent<Statue>();
        //    if (!s) continue;

        //    if (s.IsWhite())
        //        whiteCells.Add(s.CurrentCell);
        //    else
        //        blackCells.Add(s.CurrentCell);
        //}

        //if (HasLine(whiteCells))
        //{
        //    Debug.Log("White Wins!");
        //    puzzleSolved = true;
        //    LockAllPieces();
        //}
        //else if (HasLine(blackCells))
        //{
        //    Debug.Log("Black Wins!");
        //    puzzleSolved = true;
        //    LockAllPieces();
        //}
    }

    private bool HasLine(List<Vector3Int> cells)
    {
        if (cells.Count < 3) return false;

        for (int i = 0; i < cells.Count; i++)
            for (int j = i + 1; j < cells.Count; j++)
                for (int k = j + 1; k < cells.Count; k++)
                {
                    var a = cells[i];
                    var b = cells[j];
                    var c = cells[k];

                    float area = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
                    if (Mathf.Abs(area) < 0.01f) return true;
                }

        return false;
    }


    protected override void LockAllPieces()
    {
        foreach (var obj in spawnedObjs)
        {
            var s = obj.GetComponent<Statue>();
            //if (s) s.SetMove(false);
        }
    }
}
