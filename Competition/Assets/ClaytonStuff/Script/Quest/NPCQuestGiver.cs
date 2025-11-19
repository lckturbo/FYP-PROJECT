using UnityEngine;

public class NPCQuestGiver : MonoBehaviour
{
    [SerializeField] private NPCQuestDialogueData questDialogueData;

    private int currentStageIndex = 0;
    private bool playerInRange = false;
    private Quest activeQuest;

    public static bool PlayerNearNPC { get; private set; }

    private void Start()
    {
        // Load stage progress for this NPC
        currentStageIndex = QuestManager.Instance.GetNPCStage(questDialogueData.npcName);

        // Try to find an active quest that matches this NPC’s current quest stage
        if (currentStageIndex < questDialogueData.stages.Length)
        {
            var stage = questDialogueData.stages[currentStageIndex];
            if (stage.questData != null)
            {
                // Look for an active quest with the same questID
                activeQuest = QuestManager.Instance.GetActiveQuestByID(stage.questData.questID);
                if (activeQuest != null)
                {
                    // Re-link event so completion still works after scene reload
                    activeQuest.OnQuestCompleted += HandleQuestCompleted;
                }
            }
        }
    }

    private void Update()
    {
        if (!playerInRange || !Input.GetKeyDown(KeyCode.E))
            return;

        if (DialogueManager.Instance.IsDialogueActive)
        {
            DialogueManager.Instance.DisplayNextLine();
            return;
        }

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
            if (QuestManager.Instance.completedQuests.Contains(stage.questData))
            {
                Debug.Log($"Quest '{stage.questData.questName}' already completed for {questDialogueData.npcName}.");
                return;
            }

            // ?? If quest is already active, don't start it again
            if (activeQuest == null)
            {
                DialogueManager.Instance.StartDialogue(stage.startDialogue, () =>
                {
                    activeQuest = QuestManager.Instance.StartQuest(stage.questData);
                    activeQuest.OnQuestCompleted += HandleQuestCompleted;

                    Debug.Log($"Quest '{stage.questData.questName}' started AFTER dialogue.");
                });
            }

            else if (!activeQuest.isCompleted)
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

    private void HandleQuestCompleted(Quest quest)
    {
        Debug.Log($"{quest.questData.questName} completed for NPC {questDialogueData.npcName}");

        activeQuest = null;

        if (currentStageIndex < questDialogueData.stages.Length)
        {
            currentStageIndex++;
            QuestManager.Instance.SaveNPCStage(questDialogueData.npcName, currentStageIndex);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerNearNPC = true;
            playerInRange = true;
        }


    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            PlayerNearNPC = false;
        }
    }
}
