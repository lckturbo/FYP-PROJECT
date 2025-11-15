using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseMinigame : MonoBehaviour
{
    [SerializeField] protected GameObject instructionPanel;
    [SerializeField] protected TMP_Text instructionTimerText;
    [SerializeField] protected GameObject minigamePanel;
    [SerializeField] protected float instructionTime = 5f;

    [Header("Skip")]
    [SerializeField] protected Button skipButton;
    protected bool skipRequested = false;


    protected virtual void Start()
    {
        if (instructionPanel == null || instructionTimerText == null)
            return;
    }
    public MinigameManager.ResultType Result { get; protected set; } = MinigameManager.ResultType.Fail;
    public abstract IEnumerator Run();
    public void SetupSkipUI(bool show)
    {
        if (skipButton)
        {
            skipButton.onClick.RemoveListener(RequestSkip);
            if (show) skipButton.onClick.AddListener(RequestSkip);
            skipButton.gameObject.SetActive(show);
        }
        if (!show) skipRequested = false;
    }

    private void RequestSkip() => skipRequested = true;
}
