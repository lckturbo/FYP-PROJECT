using System.Collections;
using TMPro;
using UnityEngine;

public abstract class BaseMinigame : MonoBehaviour
{
    [SerializeField] protected GameObject instructionPanel;
    [SerializeField] protected TMP_Text instructionTimerText;
    [SerializeField] protected GameObject minigamePanel;
    [SerializeField] protected float instructionTime = 5f;

    protected virtual void Start()
    {
        if (instructionPanel == null || instructionTimerText == null)
            return;
    }
    public MinigameManager.ResultType Result { get; protected set; } = MinigameManager.ResultType.Fail;
    public abstract IEnumerator Run();
}
