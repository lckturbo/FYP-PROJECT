using System.Collections;
using TMPro;
using UnityEngine;

public class Deadline : BaseMinigame
{
    [Header("Gameplay Settings")]
    [SerializeField] private RectTransform[] paperParent;
    [SerializeField] private RectTransform basket;
    [SerializeField] private GameObject paperPrefab;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float fallSpeed = 300f;
    [SerializeField] private int maxMissAllowed = 3;

    private float timer;
    private bool running = false;
    private int caught = 0;
    private int missed = 0;
    public override IEnumerator Run()
    {
        BattleManager.instance?.SetBattlePaused(true);

        instructionPanel.SetActive(true);
        minigamePanel.SetActive(false);

        while (instructionTime > 0)
        {
            instructionTime -= Time.unscaledDeltaTime;
            if (instructionTimerText) instructionTimerText.text = $"Starting in... {instructionTime:F0}s";
            yield return null;
        }

        instructionPanel.SetActive(false);
        minigamePanel.SetActive(true);

        timer = 10.0f;
        running = true;
        StartCoroutine(SpawnPapers());

        while (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;
            if (timerText) timerText.text = $"{timer:F1}s";
            yield return null;
        }

        running = false;

        if (missed >= maxMissAllowed)
            Result = MinigameManager.ResultType.Fail;
        else if (caught >= 10)
            Result = MinigameManager.ResultType.Perfect;
        else
            Result = MinigameManager.ResultType.Success;

        BattleManager.instance?.SetBattlePaused(false);
    }

    private void SpawnPaper()
    {
        RectTransform area = paperParent[Random.Range(0, paperParent.Length)];

        GameObject paper = Instantiate(paperPrefab, area);
        RectTransform rt = paper.GetComponent<RectTransform>();

        float x = Random.Range(-area.rect.width / 2f + 50f, area.rect.width / 2f - 50f);
        rt.anchoredPosition = new Vector2(x, area.rect.height / 2f + 50f);

        Paper p = paper.GetComponent<Paper>();
        p.Init(this, fallSpeed, basket);
    }
    private IEnumerator SpawnPapers()
    {
        while (running)
        {
            SpawnPaper();
            yield return new WaitForSecondsRealtime(0.3f);
        }
    }

    public void OnCatch()
    {
        caught++;
        Debug.Log($"Caught paper! Total caught = {caught}");
    }

    public void OnMiss()
    {
        missed++;
        Debug.Log($"Missed paper! Total missed = {missed}");
    }
}
