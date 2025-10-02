using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleResultsUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text xpLine;
    [SerializeField] private TMP_Text levelLine;
    [SerializeField] private RectTransform statsContainer;
    [SerializeField] private GameObject statRowPrefab;   // keep this DISABLED in the prefab!
    [SerializeField] private TMP_Text enemyWarning;
    [SerializeField] private Button continueBtn;

    [Header("Style")]
    [SerializeField] private Color victoryColor = new Color(0.2f, 0.95f, 0.5f);
    [SerializeField] private Color defeatColor  = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private Color neutralStatColor = Color.white;
    [SerializeField] private Color upStatColor      = new Color(0.5f, 1f, 0.5f);
    [SerializeField] private Color downStatColor    = new Color(1f, 0.5f, 0.5f);

    [Header("Manual Layout")]
    [SerializeField] private float itemHeight;
    [SerializeField] private float spacing;
    [SerializeField] private float topPadding;

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

        // Title
        if (title)
        {
            title.text = p.playerWon ? "VICTORY" : "DEFEAT";
            title.color = p.playerWon ? victoryColor : defeatColor;
        }

        // XP
        if (xpLine)   xpLine.text   = "Gained " + p.xpGained + " XP";

        // Level
        if (levelLine) levelLine.text = "Level " + p.oldLevel + " -> Level " + p.newLevel;

        // Clear old rows (but keep the disabled template if it's a child of the container)
        if (statsContainer)
        {
            for (int i = statsContainer.childCount - 1; i >= 0; i--)
            {
                var child = statsContainer.GetChild(i).gameObject;
                if (child == statRowPrefab) continue; // keep template
                Destroy(child);
            }
        }

        // Add stat rows with manual placement
        if (statsContainer && statRowPrefab != null && p.stats != null)
        {
            for (int i = 0; i < p.stats.Count; i++)
            {
                AddStatRow(p.stats[i], i);
            }
        }

        // Enemy warning
        if (enemyWarning)
        {
            if (p.enemyScaledToLevel > 0)
            {
                enemyWarning.gameObject.SetActive(true);
                enemyWarning.text = "[WARNING: Enemies have scaled to Lv " + p.enemyScaledToLevel + "]";
            }
            else
            {
                enemyWarning.gameObject.SetActive(false);
            }
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


// ----- payload models -----
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
