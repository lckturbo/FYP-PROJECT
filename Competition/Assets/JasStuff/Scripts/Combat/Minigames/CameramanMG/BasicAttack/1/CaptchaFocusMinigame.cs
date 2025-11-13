using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CaptchaSimple : BaseMinigame
{
    [SerializeField] private CaptchaData[] datasets;

    [Header("UI Refs")]
    [SerializeField] private RectTransform gridParent;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Button verifyButton;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text timerText;

    [Header("Selection Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new(0.3f, 0.8f, 1f);

    [Header("Timer")]
    [SerializeField, Min(0.5f)] private float timeLimitSeconds = 8f;

    [Header("Animator")]
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject animationPanel;

    private CaptchaData active;
    private readonly List<Button> cells = new();
    private readonly HashSet<int> selected = new();
    private bool submitted = false;
    private float timer;

    private void Awake()
    {
        Result = MinigameManager.ResultType.Fail;
        if (verifyButton)
        {
            verifyButton.onClick.RemoveAllListeners();
            verifyButton.onClick.AddListener(OnVerify);
        }
        if (resultText) resultText.text = "";
        if (timerText) timerText.text = "";
    }

    public override IEnumerator Run()
    {
        BattleManager.instance?.SetBattlePaused(true);

        animationPanel.SetActive(true);



        if (anim)
        {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            anim.SetTrigger("start");
            yield return new WaitForSecondsRealtime(2.7f);
        }

        animationPanel.SetActive(false);

        PickDataset();
        BuildGridFromData();

        submitted = false;
        selected.Clear();
        if (resultText) resultText.text = "";

        // countdown loop
        timer = Mathf.Max(0.1f, timeLimitSeconds);
        while (!submitted && timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;
            if (timerText) timerText.text = timer.ToString("0.00");
            yield return null;
        }

        if (!submitted) OnVerify();

        // scoring
        var tier = EvaluateTier();
        switch (tier)
        {
            case Tier.Perfect: Result = MinigameManager.ResultType.Perfect; break;
            case Tier.Success: Result = MinigameManager.ResultType.Success; break;
            default: Result = MinigameManager.ResultType.Fail; break;
        }

        if (resultText) resultText.text = tier.ToString();

        yield return new WaitForSecondsRealtime(0.7f);

        BattleManager.instance?.SetBattlePaused(false);
    }

    private void PickDataset()
    {
        if (datasets != null && datasets.Length > 0)
        {
            active = datasets[Random.Range(0, datasets.Length)];
        }
        if (!active)
        {
            Debug.LogError("[CaptchaSimple] No CaptchaData available.");
        }
    }

    private void BuildGridFromData()
    {
        if (!active)
        {
            Debug.LogError("[CaptchaSimple] Active dataset is null.");
            return;
        }

        int expected = active.rows * active.cols;
        if (expected <= 0)
        {
            Debug.LogError("[CaptchaSimple] rows*cols must be > 0.");
            return;
        }
        if (active.tiles == null || active.tiles.Length != expected)
        {
            Debug.LogError($"[CaptchaSimple] tiles length {(active.tiles == null ? 0 : active.tiles.Length)} != rows*cols {expected}. " +
                           "Assign all sliced sprites in row-major order (top-left first).");
            return;
        }

        if (promptText)
            promptText.text = string.IsNullOrEmpty(active.prompt)
                ? "Select all squares with the object"
                : active.prompt;

        var existingImages = new List<Image>();
        foreach (Transform t in gridParent)
        {
            var img = t.GetComponent<Image>();
            if (img) existingImages.Add(img);
        }

        bool canReuse = existingImages.Count == expected;

        if (!canReuse)
        {
            foreach (Transform t in gridParent) Destroy(t.gameObject);
            existingImages.Clear();

            for (int i = 0; i < expected; i++)
            {
                var go = new GameObject($"Cell ({i})", typeof(RectTransform), typeof(Image), typeof(Button));
                var rt = (RectTransform)go.transform;
                rt.SetParent(gridParent, false);

                var img = go.GetComponent<Image>();
                img.type = Image.Type.Simple;
                img.preserveAspect = true;

                var btn = go.GetComponent<Button>();
                int index = i;
                btn.onClick.AddListener(() => ToggleCell(index));

                existingImages.Add(img);
            }
        }
        else
        {
            for (int i = 0; i < existingImages.Count; i++)
            {
                var img = existingImages[i];
                var btn = img.GetComponent<Button>();
                if (!btn) btn = img.gameObject.AddComponent<Button>();
                btn.onClick.RemoveAllListeners();
                int index = i;
                btn.onClick.AddListener(() => ToggleCell(index));
            }
        }

        cells.Clear();
        selected.Clear();

        for (int i = 0; i < expected; i++)
        {
            var img = existingImages[i];
            img.sprite = active.tiles[i];
            img.color = normalColor;

            var btn = img.GetComponent<Button>();
            cells.Add(btn);
        }
    }

    private void ToggleCell(int index)
    {
        if (submitted) return;

        if (selected.Contains(index))
        {
            selected.Remove(index);
            SetCellColor(index, normalColor);
        }
        else
        {
            selected.Add(index);
            SetCellColor(index, selectedColor);
        }
    }

    private void SetCellColor(int index, Color c)
    {
        if (index < 0 || index >= cells.Count) return;
        var img = cells[index] ? cells[index].GetComponent<Image>() : null;
        if (img) img.color = c;
    }

    private void OnVerify() => submitted = true;

    private enum Tier { Fail, Success, Perfect }

    private Tier EvaluateTier()
    {
        if (active == null) return Tier.Fail;

        var correct = new HashSet<int>(active.correctIndices ?? new List<int>());

        foreach (var pick in selected)
            if (!correct.Contains(pick)) return Tier.Fail;

        if (selected.Count == 0) return Tier.Fail;

        if (selected.Count == correct.Count && IsSetEqual(selected, correct))
            return Tier.Perfect;

        return Tier.Success;
    }

    private static bool IsSetEqual(HashSet<int> a, HashSet<int> b)
    {
        if (a.Count != b.Count) return false;
        foreach (var x in a) if (!b.Contains(x)) return false;
        return true;
    }
}
