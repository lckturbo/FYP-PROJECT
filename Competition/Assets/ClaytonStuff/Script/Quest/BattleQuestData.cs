using UnityEngine;

[CreateAssetMenu(fileName = "NewBattleQuest", menuName = "Quests/Battle Quest")]
public class BattleQuestData : QuestData
{
    public string targetEnemyPartyID; // The ID of the enemy party to defeat

    public override Quest CreateQuestInstance(GameObject questHolder)
    {
        var quest = questHolder.AddComponent<BattleQuest>();
        quest.questData = this;
        return quest;
    }
}
