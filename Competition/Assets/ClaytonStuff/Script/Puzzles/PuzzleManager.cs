using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [Tooltip("List of goal spots. If empty, will auto-find all PuzzleGoal in scene.")]
    public List<PuzzleGoal> goals;

    [Header("Gate / Barrier to Remove")]
    public GameObject gate;   // Assign your gate/door/barrier in the Inspector

    private void Start()
    {
        // Auto-populate if not set in inspector
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
            if (!g.IsOccupied)
            {
                // At least one goal is empty, puzzle not solved
                return;
            }
        }

        // All goals occupied
        Debug.Log("Puzzle Solved!");
        OnPuzzleSolved();
    }

    private void OnPuzzleSolved()
    {
        if (gate != null)
        {
            Destroy(gate);
            Debug.Log("Gate removed!");
        }
        else
        {
            Debug.LogWarning("Puzzle solved, but no gate assigned.");
        }
    }
}
