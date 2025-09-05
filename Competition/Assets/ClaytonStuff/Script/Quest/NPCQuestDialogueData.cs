using UnityEngine;

[System.Serializable]
public class QuestDialogueStage
{
    public DialogueData dialogue;   // Dialogue for this stage
    public QuestData questData;     // Optional quest for this stage
}

[CreateAssetMenu(fileName = "NewNPCQuestDialogue", menuName = "NPC/NPCQuestDialogue")]
public class NPCQuestDialogueData : ScriptableObject
{
    public string npcName;
    public QuestDialogueStage[] stages; // Sequential stages
}
