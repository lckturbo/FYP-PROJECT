using UnityEngine;

public class NPCQuestGiver : MonoBehaviour
{
    [SerializeField] private NPCQuestDialogueData questDialogueData;

    private int currentStageIndex = 0;
    private bool playerInRange = false;
    private Quest activeQuest;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!DialogueManager.Instance.IsDialogueActive)
            {
                // Case: all stages finished
                if (currentStageIndex >= questDialogueData.stages.Length)
                {
                    if (questDialogueData.finalDialogue != null)
                        DialogueManager.Instance.StartDialogue(questDialogueData.finalDialogue);
                    else
                        Debug.Log($"{questDialogueData.npcName} has no more quests or dialogue.");
                    return;
                }

                var stage = questDialogueData.stages[currentStageIndex];

                if (stage.questData != null)
                {
                    if (activeQuest == null) // Quest not started yet
                    {
                        DialogueManager.Instance.StartDialogue(stage.startDialogue);

                        activeQuest = QuestManagerInstance().StartQuest(stage.questData);
                        activeQuest.OnQuestCompleted += HandleQuestCompleted;
                    }
                    else if (!activeQuest.isCompleted) // Quest in progress
                    {
                        if (stage.inProgressDialogue != null)
                            DialogueManager.Instance.StartDialogue(stage.inProgressDialogue);
                        else
                            Debug.Log($"No in-progress dialogue set for {questDialogueData.npcName}");
                    }
                }
                else
                {
                    DialogueManager.Instance.StartDialogue(stage.startDialogue);
                }
            }
            else
            {
                DialogueManager.Instance.DisplayNextLine();
            }
        }
    }

    private QuestManager QuestManagerInstance()
    {
        return FindObjectOfType<QuestManager>();
    }

    private void HandleQuestCompleted(Quest quest)
    {
        Debug.Log($"{quest.questData.questName} completed for NPC {questDialogueData.npcName}");

        activeQuest = null;

        // Move to next stage
        if (currentStageIndex < questDialogueData.stages.Length)
            currentStageIndex++;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}