using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject questPanelPrefab; // prefab for each quest entry
    [SerializeField] private Transform questListParent;   // parent container (top-right UI panel)

    [Header("Quest Complete Popup")]
    [SerializeField] private GameObject questCompletePrefab; // prefab for popup
    [SerializeField] private Transform popupParent; // usually a Canvas

    private Dictionary<Quest, GameObject> activeQuestEntries = new Dictionary<Quest, GameObject>();
    private HashSet<string> shownPopups = new HashSet<string>(); // <-- new tracker

    private QuestManager questManager;

    private void Start()
    {
        questManager = FindObjectOfType<QuestManager>();

        //  Show popups only for quests that were just completed before scene change
        if (questManager != null && questManager.recentlyCompletedQuests.Count > 0)
        {
            foreach (var questData in questManager.recentlyCompletedQuests)
            {
                ShowQuestComplete(questData.questName);
            }

            // Clear once shown, so they don’t replay again later
            questManager.ClearRecentCompletions();
        }
    }


    private void Update()
    {
        if (questManager == null) return;

        // Add new quests
        foreach (var quest in questManager.activeQuests)
        {
            if (!activeQuestEntries.ContainsKey(quest))
                CreateQuestEntry(quest);
        }

        // Update quest entries
        foreach (var entry in activeQuestEntries)
        {
            TextMeshProUGUI text = entry.Value.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                string progress = entry.Key.GetProgressText();
                text.text = $"{entry.Key.questData.questName}\n<size=80%>{entry.Key.questData.description}";
                if (!string.IsNullOrEmpty(progress))
                    text.text += $"\n<color=#CCCCCC><size=75%>{progress}</size></color>";
            }
        }

        // Handle completed quests
        List<Quest> toRemove = new List<Quest>();
        foreach (var entry in activeQuestEntries)
        {
            if (entry.Key.isCompleted)
            {
                string questName = entry.Key.questData.questName;

                // Show popup only once per quest
                if (!shownPopups.Contains(questName))
                {
                    ShowQuestComplete(questName);
                    shownPopups.Add(questName);
                }

                Destroy(entry.Value);
                toRemove.Add(entry.Key);
            }
        }

        // Clean up finished quests
        foreach (var quest in toRemove)
            activeQuestEntries.Remove(quest);

        // If all active quests are gone, clear UI
        if (questManager.activeQuests.Count == 0 && activeQuestEntries.Count > 0)
        {
            foreach (var entry in activeQuestEntries.Values)
                Destroy(entry);
            activeQuestEntries.Clear();
        }
    }

    private void CreateQuestEntry(Quest quest)
    {
        GameObject entry = Instantiate(questPanelPrefab, questListParent);
        TextMeshProUGUI text = entry.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = $"{quest.questData.questName}\n<size=80%>{quest.questData.description}</size>";

        activeQuestEntries.Add(quest, entry);
    }

    private void ShowQuestComplete(string questName)
    {
        GameObject popup = Instantiate(questCompletePrefab, popupParent);
        TextMeshProUGUI text = popup.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = $"<b>Quest Complete!</b>\n{questName}";

        StartCoroutine(PopupAnimation(popup));
    }

    private System.Collections.IEnumerator PopupAnimation(GameObject popup)
    {
        RectTransform rect = popup.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = popup.AddComponent<CanvasGroup>();

        Vector3 startPos = rect.anchoredPosition;
        Vector3 endPos = startPos + Vector3.down * 100f;

        float slideDuration = 0.5f;
        float holdDuration = 1f;
        float fadeDuration = 1f;

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        yield return new WaitForSeconds(holdDuration);

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            canvasGroup.alpha = 1f - t;
            yield return null;
        }

        Destroy(popup);
    }
}
