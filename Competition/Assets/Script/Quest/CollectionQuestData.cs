using UnityEngine;

[CreateAssetMenu(fileName = "New Collection Quest", menuName = "Quests/Collection Quest")]
public class CollectionQuestData : QuestData
{
    public string itemName;
    public int requiredAmount = 5;

    public override Quest CreateQuestInstance(GameObject questHolder)
    {
        var quest = questHolder.AddComponent<CollectionQuestRuntime>();
        quest.questData = this;
        return quest;
    }
}
