using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrangeStory : BaseMinigame
{
    [Header("UI References")]
    [SerializeField] private Transform slotsParent;
    [SerializeField] private Transform piecesParent;

    private List<ArrangeSlot> slots = new();

    private void Awake()
    {
        slots.AddRange(slotsParent.GetComponentsInChildren<ArrangeSlot>());
    }
    public override IEnumerator Run()
    {
        throw new System.NotImplementedException();
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
