using UnityEngine;

[System.Serializable]
public class QuestDialogueStage
{
    public DialogueData startDialogue;       // When first talking / starting quest
    public DialogueData inProgressDialogue;  // While quest is active
    public QuestData questData;              // Quest for this stage
}

[CreateAssetMenu(fileName = "NewNPCQuestDialogue", menuName = "NPC/NPCQuestDialogue")]
public class NPCQuestDialogueData : ScriptableObject
{
    public string npcName;
    public QuestDialogueStage[] stages;

    [Header("After All Quests Finished")]
    public DialogueData finalDialogue; // Optional final dialogue after all quests
}
