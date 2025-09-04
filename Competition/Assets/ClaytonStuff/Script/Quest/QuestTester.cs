using UnityEngine;

public class QuestTestStarter : MonoBehaviour
{
    public QuestData testQuest;
    private CollectionQuestRuntime runtime;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            QuestManager qm = FindObjectOfType<QuestManager>();
            Quest startedQuest = qm.StartQuest(testQuest);

            // If the started quest is a collection quest, keep a reference
            runtime = startedQuest as CollectionQuestRuntime;
        }

        if (Input.GetKeyDown(KeyCode.R) && runtime != null)
        {
            runtime.AddItem("Heal Potion");
        }
    }
}
