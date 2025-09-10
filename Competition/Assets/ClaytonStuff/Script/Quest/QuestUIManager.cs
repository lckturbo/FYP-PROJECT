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
    [SerializeField] private float popupDuration = 2f;

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
                        $"{entry.Key.questData.description}\n" +
                        $"<size=80%>{entry.Key.questData.questName} ({entry.Key.GetProgressText()})</size>";
                }

            }
        }

        // Remove completed quests
        List<Quest> toRemove = new List<Quest>();
        foreach (var entry in activeQuestEntries)
        {
            if (entry.Key.isCompleted)
            {
                //  Show quest complete popup
                ShowQuestComplete(entry.Key.questData.questName);

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

    private void ShowQuestComplete(string questName)
    {
        GameObject popup = Instantiate(questCompletePrefab, popupParent);
        TextMeshProUGUI text = popup.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = $"<b>Quest Complete!</b>\n{questName}";
        }

        // Start animation coroutine
        StartCoroutine(PopupAnimation(popup));
    }


    private System.Collections.IEnumerator PopupAnimation(GameObject popup)
    {
        RectTransform rect = popup.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = popup.AddComponent<CanvasGroup>();

        Vector3 startPos = rect.anchoredPosition;
        Vector3 endPos = startPos + Vector3.down * 100f; // move downward

        float slideDuration = 0.5f; // time to slide down
        float holdDuration = 1f;    // time to stay in place
        float fadeDuration = 1f;    // time to fade out

        // --- Slide down ---
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        // --- Hold in place ---
        yield return new WaitForSeconds(holdDuration);

        // --- Fade out ---
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
