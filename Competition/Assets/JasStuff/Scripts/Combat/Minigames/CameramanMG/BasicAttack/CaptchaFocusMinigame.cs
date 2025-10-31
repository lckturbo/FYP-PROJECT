using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CaptchaSimple : BaseMinigame
{
    [Header("Data")]
    [SerializeField] private CaptchaData data;

    [Header("UI Refs")]
    [SerializeField] private RectTransform gridParent;   // has GridLayoutGroup
    [SerializeField] private Button cellPrefab;          // a simple Button with an Image
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Button verifyButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private TMP_Text resultText;

    [Header("Selection Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new(0.3f, 0.8f, 1f);

    // runtime
    private readonly List<Button> cells = new();
    private readonly HashSet<int> selected = new();
    private bool submitted = false;
    private bool skipped = false;

    private void Awake()
    {
        Result = MinigameManager.ResultType.Fail;
        if (verifyButton) { verifyButton.onClick.RemoveAllListeners(); verifyButton.onClick.AddListener(OnVerify); }
        if (skipButton) { skipButton.onClick.RemoveAllListeners(); skipButton.onClick.AddListener(OnSkip); }
        if (resultText) resultText.text = "";
    }

    public override System.Collections.IEnumerator Run()
    {
        BattleManager.instance?.SetBattlePaused(true);
        BuildGridFromData();
        submitted = false; skipped = false; selected.Clear();
        if (resultText) resultText.text = "";

        yield return new WaitUntil(() => submitted || skipped);

        if (skipped)
        {
            Result = MinigameManager.ResultType.Success;
            Cleanup();
            yield break;
        }

        bool correct = CheckAnswer();
        Result = correct ? MinigameManager.ResultType.Success : MinigameManager.ResultType.Fail;
        if (resultText) resultText.text = correct ? "Success!" : "Failed!";

        yield return new WaitForSecondsRealtime(1.0f);
        Cleanup();
    }

    private void BuildGridFromData()
    {
        // Clear old
        foreach (Transform c in gridParent) Destroy(c.gameObject);
        cells.Clear();
        selected.Clear();

        if (!data || data.rows * data.cols <= 0 || data.tiles == null || data.tiles.Length != data.rows * data.cols)
        {
            Debug.LogError("[CaptchaSimple] Data missing or tiles count != rows*cols.");
            return;
        }

        if (promptText) promptText.text = string.IsNullOrEmpty(data.prompt) ? "Select the correct squares" : data.prompt;

        // Create buttons row-major, top-left = 0
        int total = data.rows * data.cols;
        for (int i = 0; i < total; i++)
        {
            var btn = Instantiate(cellPrefab, gridParent);
            btn.name = $"Cell_{i}";
            if (btn.image) { btn.image.sprite = data.tiles[i]; btn.image.color = normalColor; }

            int index = i;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => ToggleCell(index));

            cells.Add(btn);
        }
    }

    private void ToggleCell(int index)
    {
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
        if (index >= 0 && index < cells.Count && cells[index] && cells[index].image)
            cells[index].image.color = c;
    }

    private void OnVerify() { submitted = true; }
    private void OnSkip() { skipped = true; submitted = true; }

    private bool CheckAnswer()
    {
        if (data == null) return false;
        if (selected.Count != data.correctIndices.Count) return false;
        foreach (int i in data.correctIndices)
            if (!selected.Contains(i)) return false;
        return true;
    }

    private void Cleanup()
    {
        BattleManager.instance?.SetBattlePaused(false);
    }
}
