using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrangeStory : BaseMinigame
{
    [SerializeField] private Animator instrAnim;
    [SerializeField] private Animator anim;
    [Header("UI References")]
    [SerializeField] private Transform slotsParent;
    [SerializeField] private Transform piecesParent;

    private List<ArrangeSlot> slots = new();
    private bool instructionStarted = false;

    private void Awake()
    {
        slots.AddRange(slotsParent.GetComponentsInChildren<ArrangeSlot>());
    }
    private void Start()
    {
        StartCoroutine(Run());
    }
    public void StartInstructionCountdown()
    {
        instructionStarted = true;
        Debug.Log("instruction started: " + instructionStarted);
    }
    public override IEnumerator Run()
    {
        BattleManager.instance?.SetBattlePaused(true);

        instructionPanel.SetActive(true);
        minigamePanel.SetActive(false);

        if (instrAnim)
        {
            anim.SetTrigger("start");
            instructionStarted = false;
            yield return null;
            yield return new WaitUntil(() => instructionStarted);
        }

        while (instructionTime > 0)
        {
            instructionTime -= Time.unscaledDeltaTime;
            if (instructionTimerText) instructionTimerText.text = $"Starting in... {instructionTime:F0}s";
            yield return null;
        }

        if (anim)
        {
            anim.enabled = true;
            yield return new WaitForSecondsRealtime(2.3f);
        }
        instructionPanel.SetActive(false);
        minigamePanel.SetActive(true);

        BattleManager.instance?.SetBattlePaused(false);
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
