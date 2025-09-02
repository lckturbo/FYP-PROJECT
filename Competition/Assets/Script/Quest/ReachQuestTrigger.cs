using UnityEngine;

public class ReachQuestTrigger : MonoBehaviour
{
    public ReachQuestData questData;
    public Transform targetLocation; // set this in the scene

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            QuestManager qm = FindObjectOfType<QuestManager>();
            Quest started = qm.StartQuest(questData);

            ReachQuestRuntime runtime = started as ReachQuestRuntime;
            if (runtime != null)
            {
                runtime.SetTarget(targetLocation);
            }

            Destroy(gameObject);
        }
    }
}
