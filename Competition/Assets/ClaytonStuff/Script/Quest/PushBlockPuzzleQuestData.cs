using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Push Block Puzzle Quest")]
public class PushBlockPuzzleQuestData : QuestData
{
    public PuzzleManager puzzlePrefab; // assign a prefab that has PuzzleManager + goals

    public override Quest CreateQuestInstance(GameObject questHolder)
    {
        PushBlockPuzzleQuest quest = questHolder.AddComponent<PushBlockPuzzleQuest>();
        quest.questData = this;
        return quest;
    }
}
