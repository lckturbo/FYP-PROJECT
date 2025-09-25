using UnityEngine;

public abstract class QuestData : ScriptableObject
{
    public string questID;   // unique ID (set in Inspector!)
    public string questName;
    [TextArea] public string description;

    public abstract Quest CreateQuestInstance(GameObject questHolder);
}
