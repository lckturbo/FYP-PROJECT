using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject questPanelPrefab; // prefab for each quest entry
    [SerializeField] private Transform questListParent;   // parent container (top-right UI panel)

    private Dictionary<Quest, GameObject> activeQuestEntries = new Dictionary<Quest, GameObject>();

    private QuestManager questManager;

    private void Start()
    {
        questManager = FindObjectOfType<QuestManager>();
    }
    private void Update()
    {
        if (questManager == null) return;

        // Add new quests
        foreach (var quest in questManager.activeQuests)
        {
            if (!activeQuestEntries.ContainsKey(quest))
            {
                CreateQuestEntry(quest);
            }
        }

        //  Update quest entries only if it's a CollectionQuestRuntime
        foreach (var entry in activeQuestEntries)
        {
            if (entry.Key is CollectionQuestRuntime) // check type
            {
                TextMeshProUGUI text = entry.Value.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text =
                        $"{entry.Key.questData.questName} ({entry.Key.GetProgressText()})\n" +
                        $"<size=80%>{entry.Key.questData.description}</size>";
                }
            }
        }

        // Remove completed quests
        List<Quest> toRemove = new List<Quest>();
        foreach (var entry in activeQuestEntries)
        {
            if (entry.Key.isCompleted)
            {
                Destroy(entry.Value);
                toRemove.Add(entry.Key);
            }
        }
        foreach (var quest in toRemove)
        {
            activeQuestEntries.Remove(quest);
        }
    }


    private void CreateQuestEntry(Quest quest)
    {
        GameObject entry = Instantiate(questPanelPrefab, questListParent);
        TextMeshProUGUI text = entry.GetComponentInChildren<TextMeshProUGUI>();

        if (text != null)
        {
            text.text = $"{quest.questData.questName}\n<size=80%>{quest.questData.description}</size>";
        }

        activeQuestEntries.Add(quest, entry);
    }
}
