using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Push Block Puzzle Quest")]
public class PushBlockPuzzleQuestData : QuestData
{
    [Tooltip("The PuzzleManager in the scene controlling this puzzle.")]
    public PuzzleManager puzzleManager;

    [Tooltip("Which puzzle group index this quest tracks (0-based).")]
    public int puzzleGroupIndex = 0;

    public override Quest CreateQuestInstance(GameObject questHolder)
    {
        PushBlockPuzzleQuest quest = questHolder.AddComponent<PushBlockPuzzleQuest>();
        quest.questData = this;
        return quest;
    }
}
