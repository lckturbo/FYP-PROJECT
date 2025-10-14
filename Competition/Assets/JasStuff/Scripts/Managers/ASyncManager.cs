using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ASyncManager : MonoBehaviour
{
    public static ASyncManager instance;

    [Header("Screens")]
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject loadingScreen;

    [Header("Hint (optional)")]
    [SerializeField] private TMP_Text hintLabel;
    [TextArea] [SerializeField] private string[] hints = new string[]
    {
        "Tip: Stronger enemies may have elemental weaknesses—experiment with different skills.",
        "Tip: Don’t forget to equip new gear after every dungeon run.",
        "Tip: You can use items outside of battle to save healing costs.",
        "Tip: Status effects like Poison and Burn can stack over turns—plan ahead.",
        "Tip: Check your party’s speed stats—turn order can make or break a fight.",
        "Tip: Save before boss rooms; sometimes the hardest battles hide behind simple doors.",
        "Tip: Some NPCs have hidden side quests if you talk to them twice.",
        "Tip: Magic isn’t always stronger—physical builds can break shields faster.",
        "Tip: Debuffs and buffs matter more than raw power against bosses.",
        "Tip: Explore every corner—you might find rare loot or secret shops.",
        "Tip: Slaying enemies will yield exp."
    };

    private bool isLoading = false;
    private float _displayedProgress = 0f;

    public static bool IsLoading => instance && instance.isLoading;
    public static float DisplayedProgress => instance ? instance._displayedProgress : 0f; // keep if any UI wants it

    public static event Action OnLoadingBegin;
    public static event Action OnLoadingEnd;

    private void Awake()
    {
        if (!instance) instance = this;
        else { Destroy(gameObject); return; }
    }

    public void LoadLevelBtn(string levelToLoad)
    {
        if (isLoading) return;
        isLoading = true;
        OnLoadingBegin?.Invoke();

        // Pick and show one random hint
        if (hintLabel && hints != null && hints.Length > 0)
            hintLabel.text = hints[UnityEngine.Random.Range(0, hints.Length)];

        var questUI = FindObjectOfType<QuestUIManager>();
        if (questUI) questUI.gameObject.SetActive(false);

        var inventoryUI = FindObjectOfType<InventoryUIManager>();
        if(inventoryUI) inventoryUI.gameObject.SetActive(false);

        if (mainScreen) mainScreen.SetActive(false);
        if (loadingScreen) loadingScreen.SetActive(true);

        StartCoroutine(LoadLevelAsync(levelToLoad));
    }

    private IEnumerator LoadLevelAsync(string levelToLoad)
    {
        float minLoadTime = 6f;
        float elapsedTime = 0f;

        AsyncOperation op = SceneManager.LoadSceneAsync(levelToLoad);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progValue = Mathf.Clamp01(op.progress / 0.9f);
            _displayedProgress = Mathf.Min(progValue, elapsedTime / minLoadTime);

            if (progValue >= 1f && elapsedTime >= minLoadTime)
                op.allowSceneActivation = true;

            yield return null;
        }

        isLoading = false;
        _displayedProgress = 1f;
        OnLoadingEnd?.Invoke();
    }
}
