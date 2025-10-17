using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PuzzleGroup
{
    [Tooltip("Goals that must all be occupied to solve this group.")]
    public List<PuzzleGoal> goals = new List<PuzzleGoal>();

    [Tooltip("Barriers or gates that will be removed when this group is solved.")]
    public List<GameObject> barriers = new List<GameObject>();

    [HideInInspector] public bool solved = false;
}

public class PuzzleManager : MonoBehaviour
{
    [Header("Puzzle Groups")]
    [Tooltip("Each group has its own set of goals and barriers.")]
    public List<PuzzleGroup> puzzleGroups = new List<PuzzleGroup>();

    public event System.Action<PuzzleGroup> OnAnyPuzzleGroupSolved;


    private void Start()
    {
        // Subscribe to events for all goals
        foreach (var group in puzzleGroups)
        {
            foreach (var goal in group.goals)
            {
                if (goal != null)
                {
                    goal.OnOccupied += (g, block) => CheckGroupSolved(group);
                    goal.OnEmptied += (g) => CheckGroupSolved(group);
                }
            }

            // Initial check (in case puzzles start already solved)
            CheckGroupSolved(group);
        }
    }

    private void CheckGroupSolved(PuzzleGroup group)
    {
        if (group.solved) return;

        foreach (var goal in group.goals)
        {
            if (goal == null || !goal.IsOccupied)
                return; // not solved yet
        }

        // All goals in this group are filled
        group.solved = true;
        OnPuzzleGroupSolved(group);
    }

    private void OnPuzzleGroupSolved(PuzzleGroup group)
    {
        Debug.Log("Puzzle group solved!");

        foreach (var barrier in group.barriers)
        {
            if (barrier != null)
            {
                Destroy(barrier);
                Debug.Log($"Removed barrier: {barrier.name}");
            }
        }

        OnAnyPuzzleGroupSolved?.Invoke(group); //  trigger quest or other systems
    }


}
