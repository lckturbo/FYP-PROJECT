using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PuzzleMove
{
    public Piece.PieceType type;
    public bool isWhite;
    public Vector3Int startCell;
    public Vector3Int targetCell;
}
[System.Serializable]
public class PuzzleClueSet
{
    public DialogueData sign1Clue;
    public DialogueData sign2Clue;
    public DialogueData sign3Clue;
}

[System.Serializable]
public class PuzzleData
{
    public List<PuzzleSolution> solutions;
}

[System.Serializable]
public class PuzzleSolution
{
    public List<PuzzleMove> moves;
    public PuzzleClueSet clues;
}

[CreateAssetMenu(fileName = "PuzzleDatabase", menuName = "ChessPuzzle/Database")]
public class PuzzleDatabase : ScriptableObject
{
    public List<PuzzleData> puzzles;
}

