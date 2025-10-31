using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public List<Quest> activeQuests = new List<Quest>();
    private List<Quest> questsToRemove = new List<Quest>();

    private Dictionary<string, int> npcQuestStages = new Dictionary<string, int>();
    public List<QuestData> completedQuests = new List<QuestData>();

    public List<QuestData> recentlyCompletedQuests = new List<QuestData>();

    private void Awake()
    {
        // If this GameObject is a child
        if (transform.parent != null)
        {
            // Detach from its parent first
            transform.SetParent(null);
        }
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
        Quest newQuest = questData.CreateQuestInstance(gameObject);
        newQuest.StartQuest();
        newQuest.OnQuestCompleted += HandleQuestCompleted;
        activeQuests.Add(newQuest);
        return newQuest;
    }

    private void HandleQuestCompleted(Quest quest)
    {
        Debug.Log($"Quest Finished: {quest.questData.questName}");
        questsToRemove.Add(quest);

        if (!completedQuests.Contains(quest.questData))
            completedQuests.Add(quest.questData);

        //  Add to recent completions (for popup display)
        if (!recentlyCompletedQuests.Contains(quest.questData))
            recentlyCompletedQuests.Add(quest.questData);
    }

    public void ClearRecentCompletions()
    {
        recentlyCompletedQuests.Clear();
    }


    private void Update()
    {
        foreach (var quest in activeQuests)
            quest.CheckProgress();

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

    public void SaveNPCStage(string npcName, int stageIndex)
    {
        npcQuestStages[npcName] = stageIndex;
    }

    public int GetNPCStage(string npcName)
    {
        return npcQuestStages.TryGetValue(npcName, out int stageIndex) ? stageIndex : 0;
    }

    //  NEW: Reset everything on game over
    public void ResetAllQuests()
    {
        Debug.Log("[QuestManager] Resetting all quests and NPC progress...");

        // Clear active quests
        foreach (var quest in activeQuests)
        {
            Destroy(quest);
        }
        activeQuests.Clear();

        // Clear completed quests
        completedQuests.Clear();

        // Reset NPC quest stages
        npcQuestStages.Clear();
    }
    public Quest GetActiveQuestByID(string questID)
    {
        foreach (var quest in activeQuests)
        {
            if (quest.questData.questID == questID)
                return quest;
        }
        return null;
    }

}
