using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleResultsUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text xpLine;       // "Gained XXX XP"
    [SerializeField] private TMP_Text levelLine;    // "Level A -> Level B"
    [SerializeField] private RectTransform statsContainer;
    [SerializeField] private GameObject statRowPrefab;   // keep DISABLED in prefab
    [SerializeField] private TMP_Text enemyWarning;
    [SerializeField] private Button continueBtn;

    [Header("Images")]
    [SerializeField] private Image titlePanelImage;
    [SerializeField] private Image backdropImage;

    [Header("Sprites")]
    [SerializeField] private Sprite victoryTitlePanelSprite;
    [SerializeField] private Sprite defeatTitlePanelSprite;
    [SerializeField] private Sprite victoryBackdropSprite;
    [SerializeField] private Sprite defeatBackdropSprite;

    [Header("Style (legacy)")]
    [SerializeField] private Color neutralStatColor = Color.white;
    [SerializeField] private Color upStatColor = new Color(0.5f, 1f, 0.5f);
    [SerializeField] private Color downStatColor = new Color(1f, 0.5f, 0.5f);

    [Header("Manual Layout")]
    [SerializeField] private float itemHeight;
    [SerializeField] private float spacing;
    [SerializeField] private float topPadding;

    [Header("XP Bar UI")]
    [Tooltip("XP progress for the CURRENT (post-battle) level")]
    [SerializeField] private Slider xpBar;               // set min=0 in prefab
    [Tooltip("e.g. '123/500 XP' or hidden if data missing")]
    [SerializeField] private TMP_Text xpBarValueLine;    // optional
    [Tooltip("e.g. '377 XP to next level' or hidden if data missing")]
    [SerializeField] private TMP_Text xpToNextLine;      // optional

    private System.Action _onContinue;
    private Color _defaultTitleColor;

    void Awake()
    {
        if (title) _defaultTitleColor = title.color;

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

        // Title (no color switching)
        if (title)
        {
            title.text = p.playerWon ? "VICTORY" : "DEFEAT";
            title.color = _defaultTitleColor;
        }

        // Title panel & backdrop sprites
        if (titlePanelImage) SetSprite(titlePanelImage, p.playerWon ? victoryTitlePanelSprite : defeatTitlePanelSprite);
        if (backdropImage) SetSprite(backdropImage, p.playerWon ? victoryBackdropSprite : defeatBackdropSprite);

        // XP gained line
        if (xpLine) xpLine.text = "Gained " + p.xpGained + " XP";

        // Level line
        if (levelLine) levelLine.text = "Level " + p.oldLevel + " -> Level " + p.newLevel;

        // XP bar
        BindXpBar(p);

        // Clear old stat rows (keep the disabled template if it's a child)
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
            for (int i = 0; i < p.stats.Count; i++) AddStatRow(p.stats[i], i);
        }

        // Enemy warning
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

    private void BindXpBar(BattleResultsPayload p)
    {
        // If we don't have XP data, hide XP UI elements gracefully
        if (!xpBar)
        {
            SafeSetActive(xpBarValueLine, false);
            SafeSetActive(xpToNextLine, false);
            return;
        }

        if (p.xpRequiredForNext <= 0)
        {
            // Missing threshold -> hide bar & companion texts
            xpBar.gameObject.SetActive(false);
            SafeSetActive(xpBarValueLine, false);
            SafeSetActive(xpToNextLine, false);
            return;
        }

        // Clamp xpAfter to [0, xpRequiredForNext)
        int clampedAfter = Mathf.Clamp(p.xpAfter, 0, p.xpRequiredForNext);

        xpBar.gameObject.SetActive(true);
        xpBar.minValue = 0;
        xpBar.maxValue = p.xpRequiredForNext;
        xpBar.value = clampedAfter;

        if (xpBarValueLine)
        {
            xpBarValueLine.gameObject.SetActive(true);
            xpBarValueLine.text = clampedAfter + " / " + p.xpRequiredForNext + " XP";
        }

        if (xpToNextLine)
        {
            xpToNextLine.gameObject.SetActive(true);
            int remaining = Mathf.Max(0, p.xpRequiredForNext - clampedAfter);
            xpToNextLine.text = remaining + " XP to next level";
        }
    }

    private static void SafeSetActive(Graphic g, bool active)
    {
        if (g) g.gameObject.SetActive(active);
    }

    private static void SetSprite(Image image, Sprite sprite)
    {
        if (!image) return;

        if (sprite)
        {
            image.enabled = true;
            image.sprite = sprite;
            // image.SetNativeSize(); // optional
        }
        else
        {
            image.enabled = false;
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

    // NEW: XP fields for the bar (post-battle state)
    // xpAfter is the XP into 'newLevel' after awarding xpGained
    public int xpBefore;           // optional (for your own use/display)
    public int xpAfter;            // required for the bar (relative to 'newLevel')
    public int xpRequiredForNext;  // required: threshold for newLevel -> next

    public int enemyScaledToLevel;
    public List<BattleResultsStat> stats;
}
