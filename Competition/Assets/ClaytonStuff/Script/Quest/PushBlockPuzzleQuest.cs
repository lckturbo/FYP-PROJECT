using UnityEngine;

public class PushBlockPuzzleQuest : Quest
{
    private PuzzleManager puzzleManager;
    private PuzzleGroup targetGroup;

    private PushBlockPuzzleQuestData Data => (PushBlockPuzzleQuestData)questData;

    public override void StartQuest()
    {
        Debug.Log($"Quest Started: {questData.questName}");

        // Get the puzzle manager from quest data
        puzzleManager = Data.puzzleManager;
        if (puzzleManager == null)
        {
            Debug.LogError("No PuzzleManager assigned in quest data!");
            return;
        }

        // Validate index
        if (Data.puzzleGroupIndex < 0 || Data.puzzleGroupIndex >= puzzleManager.puzzleGroups.Count)
        {
            Debug.LogError($"Invalid puzzle group index {Data.puzzleGroupIndex} for quest {questData.questName}");
            return;
        }

        // Get target group reference
        targetGroup = puzzleManager.puzzleGroups[Data.puzzleGroupIndex];

        // Subscribe to puzzle group solved events
        puzzleManager.OnAnyPuzzleGroupSolved += HandlePuzzleGroupSolved;

        // If already solved (e.g., loaded mid-progress)
        if (targetGroup.solved)
        {
            CompleteQuest();
        }
    }

    public override void CheckProgress()
    {
        // Handled by events
    }

    private void HandlePuzzleGroupSolved(PuzzleGroup group)
    {
        if (group == targetGroup && !isCompleted)
        {
            Debug.Log($"Puzzle group {Data.puzzleGroupIndex} solved for quest: {questData.questName}");
            CompleteQuest();
        }
    }

    private void OnDestroy()
    {
        if (puzzleManager != null)
            puzzleManager.OnAnyPuzzleGroupSolved -= HandlePuzzleGroupSolved;
    }
}
