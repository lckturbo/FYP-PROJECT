using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArrangeStory : BaseMinigame
{
    [SerializeField] private Animator instrAnim;
    [SerializeField] private Animator anim;
    [Header("UI References")]
    [SerializeField] private Transform slotsParent;
    [SerializeField] private Transform piecesParent;
    [SerializeField] private TMP_Text timerText;

    private float timer;
    //private bool running = false;
    private List<ArrangeSlot> slots = new();
    private bool instructionStarted = false;

    private void Awake()
    {
        slots.AddRange(slotsParent.GetComponentsInChildren<ArrangeSlot>(true));
        StartCoroutine(Run());
    }
    public void StartInstructionCountdown()
    {
        instructionStarted = true;
        Debug.Log("instruction started: " + instructionStarted);
    }
    public override IEnumerator Run()
    {
       // BattleManager.instance?.SetBattlePaused(true);

        instructionPanel.SetActive(true);
        minigamePanel.SetActive(false);

        SetupSkipUI(true);

        if (instrAnim)
        {
            instrAnim.SetTrigger("start");
            instructionStarted = false;
            yield return new WaitUntil(() => instructionStarted || skipRequested);

            if (skipRequested)
                instructionTime = 0f;
        }

        while (instructionTime > 0 && !skipRequested)
        {
            instructionTime -= Time.unscaledDeltaTime;
            if (instructionTimerText) instructionTimerText.text = $"Starting in... {instructionTime:F0}s";
            yield return null;
        }

        if (skipRequested)
            instructionTime = 0f;

        SetupSkipUI(false);

        if (anim)
        {
            anim.enabled = true;
            yield return new WaitForSecondsRealtime(2.3f);
        }
        instructionPanel.SetActive(false);
        minigamePanel.SetActive(true);

        timer = 20.0f;
       // running = true;

        while (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;
            if (timerText) timerText.text = $"{timer:F1}s";
            yield return null;
        }

        //running = false;

        int correctCount = 0;

        foreach (var slot in slots)
        {
            if (slot.IsCorrect())
                correctCount++;
        }

        if (correctCount == slots.Count)
            Result = MinigameManager.ResultType.Perfect;
        else if (correctCount >= Mathf.CeilToInt(slots.Count * 0.6f))
            Result = MinigameManager.ResultType.Success;
        else
            Result = MinigameManager.ResultType.Fail;

        if(timerText) timerText.text = Result + "!";
        yield return new WaitForSecondsRealtime(1.0f);

        //BattleManager.instance?.SetBattlePaused(false);
    }
    public void OnSubmit()
    {
        int correctCount = 0;

        foreach (var slot in slots)
        {
            if (slot.IsCorrect())
                correctCount++;
        }

        if (correctCount == slots.Count)
            Result = MinigameManager.ResultType.Perfect;
        else if (correctCount >= Mathf.CeilToInt(slots.Count * 0.6f))
            Result = MinigameManager.ResultType.Success;
        else
            Result = MinigameManager.ResultType.Fail;

        Debug.Log(Result);
    }
}
