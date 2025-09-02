using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public List<Quest> activeQuests = new List<Quest>();

    public Quest StartQuest(QuestData questData)
    {
        Quest newQuest = questData.CreateQuestInstance(gameObject);
        newQuest.StartQuest();
        newQuest.OnQuestCompleted += HandleQuestCompleted;
        activeQuests.Add(newQuest);
        return newQuest; // <-- return the quest instance
    }


    private void HandleQuestCompleted(Quest quest)
    {
        Debug.Log($"Quest Finished: {quest.questData.questName}");
        activeQuests.Remove(quest);
        Destroy(quest);
    }

    private void Update()
    {
        foreach (var quest in activeQuests)
        {
            quest.CheckProgress();
        }
    }
}
