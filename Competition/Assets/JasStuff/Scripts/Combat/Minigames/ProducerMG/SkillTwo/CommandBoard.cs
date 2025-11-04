using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandBoard : BaseMinigame
{
    [Header("RTS Board Reference")]
    [SerializeField] private RectTransform boardParent;

    private bool running = false;
    private float timer;
    public override IEnumerator Run()
    {
        yield return null;
    }
    private void StartPlanning()
    {
        running = true;

    }
    public void ConfirmPlan()
    {

    }
}
