using System.Collections;
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
    [SerializeField] private GameObject statRowPrefab;
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

    [Header("Victory/Defeat")]
    [SerializeField] private Image xpBarBackground;
    [SerializeField] private Sprite victoryXpBarBG;
    [SerializeField] private Sprite defeatXpBarBG;

    [Header("Style")]
    [SerializeField] private Color neutralStatColor = Color.white;
    [SerializeField] private Color upStatColor = new Color(0.5f, 1f, 0.5f);
    [SerializeField] private Color downStatColor = new Color(1f, 0.5f, 0.5f);

    [Header("Manual Layout")]
    [SerializeField] private float itemHeight;
    [SerializeField] private float spacing;
    [SerializeField] private float topPadding;

    [Header("XP Bar UI")]
    [SerializeField] private Slider xpBar;
    [SerializeField] private TMP_Text xpBarValueLine;
    [SerializeField] private TMP_Text xpToNextLine;

    [Header("XP Animation")]
    [SerializeField] private bool animateXpBar = true;
    [SerializeField, Min(0.01f)] private float xpBarSegmentDuration = 2.0f;

    [SerializeField] private bool autoScaleSpeed = true;
    [SerializeField, Min(0.01f)] private float minSegmentDuration = 1.2f;
    [SerializeField, Min(0.01f)] private float maxSegmentDuration = 3.0f;
    [SerializeField, Min(0.25f)] private float levelsForMaxDuration = 4.0f;

    [SerializeField] private AnimationCurve xpBarCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private System.Action _onContinue;
    private Color _defaultTitleColor;
    private Coroutine _xpAnimCo;

    void Awake()
    {
        if (title) _defaultTitleColor = title.color;

        if (panel) panel.SetActive(false);
        if (continueBtn) continueBtn.onClick.AddListener(() =>
        {
            if (_xpAnimCo != null)
            {
                StopCoroutine(_xpAnimCo);
                _xpAnimCo = null;
            }
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

        if (title)
        {
            title.text = p.playerWon ? "Victory" : "Defeat";
            title.color = _defaultTitleColor;
        }

        if (titlePanelImage) SetSprite(titlePanelImage, p.playerWon ? victoryTitlePanelSprite : defeatTitlePanelSprite);
        if (backdropImage) SetSprite(backdropImage, p.playerWon ? victoryBackdropSprite : defeatBackdropSprite);

        if (xpBarBackground) SetSprite(xpBarBackground, p.playerWon ? victoryXpBarBG : defeatXpBarBG);

        if (xpLine) xpLine.text = "Gained " + p.xpGained + " XP";
        if (levelLine) levelLine.text = "Level " + p.oldLevel + " -> Level " + p.newLevel;

        BindXpBar(p);

        if (statsContainer)
        {
            for (int i = statsContainer.childCount - 1; i >= 0; i--)
            {
                var child = statsContainer.GetChild(i).gameObject;
                if (child == statRowPrefab) continue;
                Destroy(child);
            }
        }

        if (statsContainer && statRowPrefab != null && p.stats != null)
        {
            for (int i = 0; i < p.stats.Count; i++) AddStatRow(p.stats[i], i);
        }

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

    // ---------- XP bar binding & animation ----------
    private void BindXpBar(BattleResultsPayload p)
    {
        if (!xpBar)
        {
            SafeSetActive(xpBarValueLine, false);
            SafeSetActive(xpToNextLine, false);
            return;
        }

        // if new level invalid, hide
        if (p.newLevel < 1)
        {
            xpBar.gameObject.SetActive(false);
            SafeSetActive(xpBarValueLine, false);
            SafeSetActive(xpToNextLine, false);
            return;
        }

        if (_xpAnimCo != null)
        {
            StopCoroutine(_xpAnimCo);
            _xpAnimCo = null;
        }

        if (!animateXpBar || p.oldLevel <= 0)
        {
            // snap to final
            int req = RequiredXP(p.newLevel);
            int clampedAfter = Mathf.Clamp(p.xpAfter, 0, req);
            ApplyXpBarVisuals(clampedAfter, req);
            return;
        }

        _xpAnimCo = StartCoroutine(AnimateXpFill(p));
    }

    private IEnumerator AnimateXpFill(BattleResultsPayload p)
    {
        xpBar.gameObject.SetActive(true);

        int startLevel = Mathf.Max(1, p.oldLevel);
        int endLevel = Mathf.Max(1, p.newLevel);

        if (endLevel < startLevel)
        {
            // weird edge case: snap and exit
            int reqSnap = RequiredXP(p.newLevel);
            ApplyXpBarVisuals(Mathf.Clamp(p.xpAfter, 0, reqSnap), reqSnap);
            _xpAnimCo = null;
            yield break;
        }

        float baseDur = autoScaleSpeed ? ComputeAutoBaseDuration(p) : xpBarSegmentDuration;

        // 1) First segment: from current XP within startLevel to either level-up or final if no level change
        int segStartLevel = startLevel;
        int segStartFrom = Mathf.Clamp(p.xpBefore, 0, RequiredXP(segStartLevel));

        if (endLevel == startLevel)
        {
            // animate within the same level from xpBefore -> xpAfter
            int to = Mathf.Clamp(p.xpAfter, 0, RequiredXP(segStartLevel));
            float frac = Mathf.Clamp01(Mathf.Abs(to - segStartFrom) / (float)Mathf.Max(1, RequiredXP(segStartLevel)));
            yield return AnimateSegment(segStartLevel, segStartFrom, to, baseDur * Mathf.Max(0.15f, frac));
            _xpAnimCo = null;
            yield break;
        }
        else
        {
            // fill to level-up
            int segTarget = RequiredXP(segStartLevel);
            float fracFirst = Mathf.Clamp01((segTarget - segStartFrom) / (float)segTarget);
            if (segStartFrom < segTarget)
            {
                yield return AnimateSegment(segStartLevel, segStartFrom, segTarget, baseDur * Mathf.Max(0.15f, fracFirst));
            }
        }

        // 2) Intermediate full levels (if any)
        for (int lvl = startLevel + 1; lvl < endLevel; lvl++)
        {
            yield return AnimateSegment(lvl, 0, RequiredXP(lvl), baseDur);
        }

        // 3) Final partial on endLevel from 0 -> xpAfter
        int finalReq = RequiredXP(endLevel);
        int finalTo = Mathf.Clamp(p.xpAfter, 0, finalReq);
        float fracLast = Mathf.Clamp01(finalTo / (float)finalReq);
        yield return AnimateSegment(endLevel, 0, finalTo, baseDur * Mathf.Max(0.15f, fracLast));

        _xpAnimCo = null;
    }

    private float ComputeAutoBaseDuration(BattleResultsPayload p)
    {
        int startLevel = Mathf.Max(1, p.oldLevel);
        int endLevel = Mathf.Max(1, p.newLevel);

        float firstReq = RequiredXP(startLevel);
        float firstPart = Mathf.Clamp01((firstReq - Mathf.Clamp(p.xpBefore, 0, (int)firstReq)) / Mathf.Max(1f, firstReq));

        float intermediates = Mathf.Max(0, endLevel - startLevel - 1);

        float lastReq = RequiredXP(endLevel);
        float lastPart = (endLevel >= startLevel)
            ? Mathf.Clamp01(Mathf.Clamp(p.xpAfter, 0, (int)lastReq) / Mathf.Max(1f, lastReq))
            : 0f;

        float levelEquivalents = firstPart + intermediates + lastPart;

        float t = Mathf.Clamp01(levelEquivalents / Mathf.Max(0.0001f, levelsForMaxDuration));
        return Mathf.Lerp(minSegmentDuration, maxSegmentDuration, t);
    }

    private IEnumerator AnimateSegment(int level, int from, int to, float duration)
    {
        int req = Mathf.Max(1, RequiredXP(level));

        xpBar.minValue = 0;
        xpBar.maxValue = req;

        float t = 0f;
        ApplyXpBarVisuals(from, req);

        float dur = Mathf.Max(0.01f, duration);

        while (t < 1f)
        {
            t += (Time.unscaledDeltaTime / dur);
            float tt = xpBarCurve != null ? Mathf.Clamp01(xpBarCurve.Evaluate(Mathf.Clamp01(t))) : Mathf.Clamp01(t);
            int v = Mathf.RoundToInt(Mathf.Lerp(from, to, tt));
            ApplyXpBarVisuals(v, req);
            yield return null;
        }

        ApplyXpBarVisuals(to, req);
    }

    private void ApplyXpBarVisuals(int current, int required)
    {
        xpBar.gameObject.SetActive(true);
        xpBar.minValue = 0;
        xpBar.maxValue = required;
        xpBar.value = Mathf.Clamp(current, 0, required);

        if (xpBarValueLine)
        {
            xpBarValueLine.gameObject.SetActive(true);
            xpBarValueLine.text = current + " / " + required + " XP";
        }

        if (xpToNextLine)
        {
            xpToNextLine.gameObject.SetActive(true);
            int remaining = Mathf.Max(0, required - current);
            xpToNextLine.text = remaining + " XP to next level";
        }
    }

    // ---------- helpers ----------
    private static int RequiredXP(int level)
    {
        level = Mathf.Max(1, level);
        return 50 + (level * level * 20);
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
        }
        else
        {
            image.enabled = false;
        }
    }

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

// ----- payload -----
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

    public int xpBefore;
    public int xpAfter;
    public int xpRequiredForNext;

    public int enemyScaledToLevel;
    public List<BattleResultsStat> stats;
}
