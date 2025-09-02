using UnityEngine;

public abstract class QuestData : ScriptableObject
{
    public string questName;
    [TextArea] public string description;

    public abstract Quest CreateQuestInstance(GameObject questHolder);
}
