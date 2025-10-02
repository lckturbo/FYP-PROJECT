using UnityEngine;

[CreateAssetMenu(fileName = "NewBattleQuest", menuName = "Quests/Battle Quest")]
public class BattleQuestData : QuestData
{
    [Tooltip("Target enemy party IDs to defeat")]
    public string[] targetEnemyPartyIDs;

    public override Quest CreateQuestInstance(GameObject questHolder)
    {
        var quest = questHolder.AddComponent<BattleQuest>();
        quest.questData = this;
        return quest;
    }
}
