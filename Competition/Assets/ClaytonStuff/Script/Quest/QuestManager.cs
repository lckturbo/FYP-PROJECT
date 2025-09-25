using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public List<Quest> activeQuests = new List<Quest>();
    private List<Quest> questsToRemove = new List<Quest>(); // buffer for removals

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // ensure only one manager exists
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Quest StartQuest(QuestData questData)
    {
        Quest newQuest = questData.CreateQuestInstance(gameObject);
        newQuest.StartQuest();
        newQuest.OnQuestCompleted += HandleQuestCompleted;
        activeQuests.Add(newQuest);
        return newQuest;
    }

    private void HandleQuestCompleted(Quest quest)
    {
        Debug.Log($"Quest Finished: {quest.questData.questName}");
        questsToRemove.Add(quest); // mark for removal, don’t remove immediately
    }

    private void Update()
    {
        // Update all quests
        foreach (var quest in activeQuests)
        {
            quest.CheckProgress();
        }

        // Remove completed quests safely after loop
        if (questsToRemove.Count > 0)
        {
            foreach (var quest in questsToRemove)
            {
                activeQuests.Remove(quest);
                Destroy(quest);
            }
            questsToRemove.Clear();
        }
    }
}
