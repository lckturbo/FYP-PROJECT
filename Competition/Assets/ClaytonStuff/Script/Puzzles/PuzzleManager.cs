using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PuzzleGroup
{
    [Tooltip("Unique ID for saving/loading this puzzle group")]
    public string puzzleID;

    [Tooltip("Goals that must all be occupied to solve this group.")]
    public List<PuzzleGoal> goals = new List<PuzzleGoal>();

    [Tooltip("Barriers or gates that will be removed when this group is solved.")]
    public List<GameObject> barriers = new List<GameObject>();

    [Tooltip("Optional animators to trigger when solved")]
    public List<Animator> animators = new List<Animator>();

    [HideInInspector] public bool solved = false;
}

public class PuzzleManager : MonoBehaviour, IDataPersistence
{
    [Header("Puzzle Groups")]
    [Tooltip("Each group has its own set of goals and barriers.")]
    public List<PuzzleGroup> puzzleGroups = new List<PuzzleGroup>();

    public event System.Action<PuzzleGroup> OnAnyPuzzleGroupSolved;

    private void Start()
    {
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

        group.solved = true;
        OnPuzzleGroupSolved(group);
    }

    private void OnPuzzleGroupSolved(PuzzleGroup group)
    {
        Debug.Log($"Puzzle group solved: {group.puzzleID}");

        foreach (var barrier in group.barriers)
        {
            if (barrier != null)
                Destroy(barrier);
        }

        foreach (var anim in group.animators)
        {
            if (anim != null)
                anim.SetTrigger("Solved");
        }

        OnAnyPuzzleGroupSolved?.Invoke(group);
    }

    // ====== SAVE / LOAD ======

    public void SaveData(ref GameData data)
    {
        foreach (var group in puzzleGroups)
        {
            var existing = data.savedPuzzles.Find(p => p.puzzleID == group.puzzleID);
            if (existing != null)
            {
                existing.solved = group.solved;
            }
            else
            {
                data.savedPuzzles.Add(new GameData.PuzzleSaveEntry
                {
                    puzzleID = group.puzzleID,
                    solved = group.solved
                });
            }
        }
    }

    public void LoadData(GameData data)
    {
        foreach (var entry in data.savedPuzzles)
        {
            var group = puzzleGroups.Find(g => g.puzzleID == entry.puzzleID);
            if (group == null) continue;

            group.solved = entry.solved;

            if (group.solved)
            {
                // Reapply the solved state
                foreach (var barrier in group.barriers)
                {
                    if (barrier != null)
                        Destroy(barrier);
                }

                foreach (var anim in group.animators)
                {
                    if (anim != null)
                        anim.Play("Solved", 0, 1f);
                }
            }
        }
    }
}
