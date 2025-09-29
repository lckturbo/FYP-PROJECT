using UnityEngine;

public class NPCQuestGiver : MonoBehaviour
{
    [SerializeField] private NPCQuestDialogueData questDialogueData;

    private int currentStageIndex = 0;
    private bool playerInRange = false;
    private Quest activeQuest;

    private void Start()
    {
        // Load stage progress for this NPC
        currentStageIndex = QuestManager.Instance.GetNPCStage(questDialogueData.npcName);
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!DialogueManager.Instance.IsDialogueActive)
            {
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
                    if (activeQuest == null)
                    {
                        DialogueManager.Instance.StartDialogue(stage.startDialogue);

                        activeQuest = QuestManagerInstance().StartQuest(stage.questData);
                        activeQuest.OnQuestCompleted += HandleQuestCompleted;
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
            else
            {
                DialogueManager.Instance.DisplayNextLine();
            }
        }
    }

    private QuestManager QuestManagerInstance()
    {
        return QuestManager.Instance;
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
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
