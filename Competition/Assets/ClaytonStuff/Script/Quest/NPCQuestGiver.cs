using UnityEngine;

public class NPCQuestGiver : MonoBehaviour
{
    [SerializeField] private NPCQuestDialogueData questDialogueData;

    private int currentStageIndex = 0;
    private bool playerInRange = false;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!DialogueManager.Instance.IsDialogueActive)
            {
                var stage = questDialogueData.stages[currentStageIndex];
                DialogueManager.Instance.StartDialogue(stage.dialogue);

                // If this stage has a quest and it's not already active, start it
                if (stage.questData != null)
                {
                    Quest quest = QuestManagerInstance().StartQuest(stage.questData);
                    quest.OnQuestCompleted += HandleQuestCompleted;
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

        // Move to next dialogue stage
        if (currentStageIndex < questDialogueData.stages.Length - 1)
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
