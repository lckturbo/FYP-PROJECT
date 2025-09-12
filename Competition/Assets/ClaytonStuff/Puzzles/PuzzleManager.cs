using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public List<PuzzleGoal> goals;

    private void Start()
    {
        if (goals == null || goals.Count == 0)
            goals = new List<PuzzleGoal>(FindObjectsOfType<PuzzleGoal>());

        foreach (var g in goals)
        {
            g.OnOccupied += HandleGoalOccupied;
            g.OnEmptied += HandleGoalEmptied;
        }
        CheckSolved();
    }

    private void HandleGoalOccupied(PuzzleGoal g, PushableBlock block)
    {
        CheckSolved();
    }

    private void HandleGoalEmptied(PuzzleGoal g)
    {
        CheckSolved();
    }

    private void CheckSolved()
    {
        foreach (var g in goals)
        {
            if (!g.IsOccupied) return;
        }

        // All goals occupied
        Debug.Log("Puzzle Solved!");
        OnPuzzleSolved();
    }

    private void OnPuzzleSolved()
    {
        // TODO: do whatever (open door, spawn reward, etc.)
    }
}
