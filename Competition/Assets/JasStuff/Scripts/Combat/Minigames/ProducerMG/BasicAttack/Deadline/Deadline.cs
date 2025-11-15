using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Deadline : BaseMinigame
{
    [Header("Animators")]
    [SerializeField] private Animator anim;
    [SerializeField] private Animator instrAnim;

    [Header("References")]
    [SerializeField] private RectTransform[] paperParent;
    [SerializeField] private RectTransform basket;
    [SerializeField] private GameObject paperPrefab;

    [Header("Texts")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text caughtText;
    [SerializeField] private TMP_Text missedText;

    [Header("Gameplay Settings")]
    [SerializeField] private float fallSpeed = 300f;
    [SerializeField] private int maxMissAllowed = 3;

    private float timer;
    private bool running = false;
    private int caught = 0;
    private int missed = 0;

    private bool instructionStarted = false;
    private readonly List<GameObject> activePapers = new();

    public void StartInstructionCountdown()
    {
        instructionStarted = true;
        Debug.Log("instruction started: " + instructionStarted);
    }

    public override IEnumerator Run()
    {
        BattleManager.instance?.SetBattlePaused(true);
        SetupSkipUI(true);

        if (anim)
        {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            anim.SetTrigger("start");
            yield return new WaitForSecondsRealtime(2.5f);
        }

        instructionPanel.SetActive(true);
        instrAnim.enabled = true;
        minigamePanel.SetActive(false);

        instructionStarted = false;
        yield return null;
        while (!instructionStarted && !skipRequested) yield return null;

        while (instructionTime > 0 && !skipRequested)
        {
            instructionTime -= Time.unscaledDeltaTime;
            if (instructionTimerText) instructionTimerText.text = $"Starting in... {instructionTime:F0}s";
            yield return null;
        }

        if (skipRequested) instructionTime = 0f;

        instructionPanel.SetActive(false);
        minigamePanel.SetActive(true);
        SetupSkipUI(false);

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
        foreach (var paper in activePapers)
        {
            if (paper != null)
                Destroy(paper);
        }
        activePapers.Clear();

        basket.GetComponent<MonoBehaviour>().enabled = false;

        if (missed >= maxMissAllowed) // 3 and below
            Result = MinigameManager.ResultType.Fail;
        else if (caught >= 10)
            Result = MinigameManager.ResultType.Perfect;
        else // 11 and above
            Result = MinigameManager.ResultType.Success;

        if (caughtText) caughtText.text = Result + "!";
        yield return new WaitForSecondsRealtime(1.0f);

        BattleManager.instance?.SetBattlePaused(false);
    }

    private void SpawnPaper()
    {
        RectTransform area = paperParent[Random.Range(0, paperParent.Length)];

        GameObject paper = Instantiate(paperPrefab, area);
        activePapers.Add(paper);
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
            yield return new WaitForSecondsRealtime(0.35f);
        }
    }

    public void RemovePaper(GameObject paper)
    {
        activePapers.Remove(paper);
    }
    public void OnCatch()
    {
        caught++;
        SetCaughtText(caught.ToString());
    }

    public void OnMiss()
    {
        missed++;
        StartCoroutine(SetMissedText());
        SetCaughtText(caught.ToString());
    }

    private void SetCaughtText(string text)
    {
        if (caughtText) caughtText.text = "Caught: " + text;
    }

    private IEnumerator SetMissedText()
    {
        missedText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(0.5f);
        missedText.gameObject.SetActive(false);
    }
}
