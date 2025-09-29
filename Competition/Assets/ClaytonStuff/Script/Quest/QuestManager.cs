using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public List<Quest> activeQuests = new List<Quest>();
    private List<Quest> questsToRemove = new List<Quest>();

    // NEW: Track NPC progress across scenes
    private Dictionary<string, int> npcQuestStages = new Dictionary<string, int>();

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

    // ---- NPC Progress Functions ----
    public void SaveNPCStage(string npcName, int stageIndex)
    {
        npcQuestStages[npcName] = stageIndex;
    }

    public int GetNPCStage(string npcName)
    {
        return npcQuestStages.TryGetValue(npcName, out int stageIndex) ? stageIndex : 0;
    }
}
