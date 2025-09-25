using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public List<Quest> activeQuests = new List<Quest>();
    private List<Quest> questsToRemove = new List<Quest>();
    private HashSet<string> completedQuests = new HashSet<string>(); //  NEW

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Quest StartQuest(QuestData questData)
    {
        if (IsQuestCompleted(questData))
        {
            Debug.Log($"Quest {questData.questName} is already completed. Skipping.");
            return null;
        }

        Quest newQuest = questData.CreateQuestInstance(gameObject);
        newQuest.StartQuest();
        newQuest.OnQuestCompleted += HandleQuestCompleted;
        activeQuests.Add(newQuest);
        return newQuest;
    }

    private void HandleQuestCompleted(Quest quest)
    {
        Debug.Log($"Quest Finished: {quest.questData.questName}");

        completedQuests.Add(quest.questData.questID); //  track by ID
        questsToRemove.Add(quest);
    }

    private void Update()
    {
        foreach (var quest in activeQuests)
        {
            quest.CheckProgress();
        }

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

    // === NEW HELPERS ===
    public bool IsQuestCompleted(QuestData questData)
    {
        return completedQuests.Contains(questData.questID);
    }
}
