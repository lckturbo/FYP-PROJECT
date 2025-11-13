using UnityEngine;

[CreateAssetMenu(fileName = "NewBattleQuest", menuName = "Quests/Battle Quest")]
public class BattleQuestData : QuestData
{
    [Tooltip("How many enemies must be defeated for this quest.")]
    public int requiredKills = 3;

    public override Quest CreateQuestInstance(GameObject questHolder)
    {
        var quest = questHolder.AddComponent<BattleQuest>();
        quest.questData = this;
        return quest;
    }
}
