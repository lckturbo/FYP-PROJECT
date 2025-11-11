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

    [Header("Slide Settings")]
    [SerializeField] private RectTransform questListRect; // Assign questListParent here in Inspector
    [SerializeField] private float slideDistance = 400f;   // How far to slide right
    [SerializeField] private float slideSpeed = 6f;

    private bool isHidden = false;
    private Vector2 shownPosition;
    private Vector2 hiddenPosition;

    private QuestManager questManager;

    private void Awake()
    {
        if (questListParent != null)
        {
            questListRect = questListParent.GetComponent<RectTransform>();
            shownPosition = questListRect.anchoredPosition;
            hiddenPosition = shownPosition + new Vector2(slideDistance, 0f);
        }
    }

    private void Start()
    {
        questManager = FindObjectOfType<QuestManager>();

        //  Only show popup if a BattleQuest just finished before this scene
        if (BattleQuest.ShouldShowBattlePopupAfterReload && questManager != null)
        {
            foreach (var questData in questManager.completedQuests)
            {
                // Only show popup for battle quests (optional name check)
                if (questData is BattleQuestData)
                {
                    ShowQuestComplete(questData.questName);
                    break; // only once
                }
            }

            // Reset flag so it doesn’t trigger again
            BattleQuest.ClearBattlePopupFlag();
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
                    text.text += $"\n<color=#FF0000><size=75%>{progress}</size></color>";
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

    private void LateUpdate()
    {
        HandleQuestToggle();

        // Smooth slide transition
        if (questListRect != null)
        {
            Vector2 target = isHidden ? hiddenPosition : shownPosition;
            questListRect.anchoredPosition = Vector2.Lerp(
                questListRect.anchoredPosition,
                target,
                Time.unscaledDeltaTime * slideSpeed
            );
        }
    }

    private void HandleQuestToggle()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isHidden = !isHidden;
        }
    }
}
