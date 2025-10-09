using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleResultsUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private GameObject panel;

    [SerializeField] private Image titleBanner;

    // Backdrop image on the results panel
    [SerializeField] private Image backdrop;

    [SerializeField] private TMP_Text xpLine;
    [SerializeField] private TMP_Text levelLine;
    [SerializeField] private RectTransform statsContainer;
    [SerializeField] private GameObject statRowPrefab;   // keep DISABLED in the prefab!
    [SerializeField] private TMP_Text enemyWarning;
    [SerializeField] private Button continueBtn;

    [Header("Sprites")]
    [SerializeField] private Sprite victoryBannerSprite;
    [SerializeField] private Sprite defeatBannerSprite;
    [SerializeField] private Sprite victoryBackdropSprite;
    [SerializeField] private Sprite defeatBackdropSprite;

    [Header("Row Colors")]
    [SerializeField] private Color neutralStatColor = Color.white;
    [SerializeField] private Color upStatColor = new Color(0.5f, 1f, 0.5f);
    [SerializeField] private Color downStatColor = new Color(1f, 0.5f, 0.5f);

    [Header("Manual Layout")]
    [SerializeField] private float itemHeight = 28f;
    [SerializeField] private float spacing = 4f;
    [SerializeField] private float topPadding = 8f;

    private System.Action _onContinue;

    void Awake()
    {
        if (panel) panel.SetActive(false);
        if (continueBtn) continueBtn.onClick.AddListener(() =>
        {
            if (panel) panel.SetActive(false);
            _onContinue?.Invoke();
            _onContinue = null;
        });
    }

    public void Show(BattleResultsPayload p, System.Action onContinue)
    {
        _onContinue = onContinue;
        if (!panel) return;
        panel.SetActive(true);

        // ---- NEW: swap sprites for banner and backdrop ----
        if (titleBanner)
            titleBanner.sprite = p.playerWon ? victoryBannerSprite : defeatBannerSprite;

        if (backdrop)
            backdrop.sprite = p.playerWon ? victoryBackdropSprite : defeatBackdropSprite;

        // XP & Level text (unchanged)
        if (xpLine) xpLine.text = "Gained " + p.xpGained + " XP";
        if (levelLine) levelLine.text = "Level " + p.oldLevel + " -> Level " + p.newLevel;

        // Clear old rows (keep disabled template)
        if (statsContainer)
        {
            for (int i = statsContainer.childCount - 1; i >= 0; i--)
            {
                var child = statsContainer.GetChild(i).gameObject;
                if (child == statRowPrefab) continue;
                Destroy(child);
            }
        }

        // Add stat rows
        if (statsContainer && statRowPrefab != null && p.stats != null)
        {
            for (int i = 0; i < p.stats.Count; i++)
                AddStatRow(p.stats[i], i);
        }

        // Enemy scaling warning (unchanged)
        if (enemyWarning)
        {
            if (p.enemyScaledToLevel > 0)
            {
                enemyWarning.gameObject.SetActive(true);
                enemyWarning.text = "[WARNING: Enemies have scaled to Lv " + p.enemyScaledToLevel + "]";
            }
            else enemyWarning.gameObject.SetActive(false);
        }
    }

    // ---- Manual placement helper ----
    public void AddStatRow(BattleResultsStat stat, int index)
    {
        var go = Instantiate(statRowPrefab, statsContainer);
        go.SetActive(true);

        var rt = go.GetComponent<RectTransform>();
        float step = itemHeight + spacing;
        float y = -topPadding - index * step;
        rt.anchoredPosition = new Vector2(0f, y);

        var row = go.GetComponent<StatRowUI>() ?? go.AddComponent<StatRowUI>();
        bool up = stat.newValue > stat.oldValue;
        bool down = stat.newValue < stat.oldValue;
        var color = up ? upStatColor : (down ? downStatColor : neutralStatColor);
        row.Bind(stat.label, stat.oldValueText, stat.newValueText, color);
    }
}

// ----- payload models (unchanged) -----
[System.Serializable]
public struct BattleResultsStat
{
    public string label;
    public float oldValue;
    public float newValue;
    public string oldValueText;
    public string newValueText;
}

[System.Serializable]
public struct BattleResultsPayload
{
    public bool playerWon;
    public int xpGained;
    public int oldLevel;
    public int newLevel;
    public int enemyScaledToLevel;
    public List<BattleResultsStat> stats;
}
