using UnityEngine;

[CreateAssetMenu(fileName = "New Reach Quest", menuName = "Quests/Reach Quest")]
public class ReachQuestData : QuestData
{
    [Tooltip("The name of the target object in the scene.")]
    public string targetObjectName;
    public float reachRadius = 2f;

    public override Quest CreateQuestInstance(GameObject questHolder)
    {
        var quest = questHolder.AddComponent<ReachQuestRuntime>();
        quest.questData = this;
        return quest;
    }
}
