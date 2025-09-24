using UnityEngine;

public class PushBlockPuzzleQuest : Quest
{
    private PuzzleManager puzzleManager;

    public override void StartQuest()
    {
        Debug.Log($"Quest Started: {questData.questName}");

        // Find existing PuzzleManager in scene
        puzzleManager = FindObjectOfType<PuzzleManager>();
        if (puzzleManager == null)
        {
            Debug.LogError("No PuzzleManager found in scene for PushBlockPuzzleQuest!");
            return;
        }

        // Subscribe to puzzle solved
        puzzleManager.OnPuzzleSolvedEvent += HandlePuzzleSolved;
    }

    public override void CheckProgress()
    {
        // No per-frame checks needed, PuzzleManager triggers event
    }

    private void HandlePuzzleSolved()
    {
        if (!isCompleted)
        {
            CompleteQuest();
        }
    }

    private void OnDestroy()
    {
        if (puzzleManager != null)
            puzzleManager.OnPuzzleSolvedEvent -= HandlePuzzleSolved;
    }
}
