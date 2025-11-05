using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TakeABreak : BaseMinigame
{
    [SerializeField] private GameObject animPanel;
    [SerializeField] private Animator anim;
    [SerializeField] private Button rollButton;
    [SerializeField] private TMP_Text resultText;

    private bool rolled;

    private void Start()
    {
        rollButton.onClick.AddListener(OnRoll);
        resultText.text = "Press ROLL!";
    }
    private void OnRoll()
    {
        int roll = Random.Range(1, 101);
        resultText.text = $"Rolled: {roll}";

        if (roll <= 1)
            Result = MinigameManager.ResultType.Perfect;
        else if (roll <= 3)
            Result = MinigameManager.ResultType.Success;
        else
            Result = MinigameManager.ResultType.Fail;

        rolled = true;
    }
    public override IEnumerator Run()
    {
        BattleManager.instance?.SetBattlePaused(true);
        if (anim)
        {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            anim.SetTrigger("start");
            yield return new WaitForSecondsRealtime(2.2f);
        }

        animPanel.SetActive(false);
        //instructionPanel.SetActive(true);
        minigamePanel.SetActive(false);

        //if (instructionPanel.activeSelf)
        //{
        //    while (instructionTime > 0)
        //    {
        //        instructionTime -= Time.unscaledDeltaTime;
        //        if (instructionTimerText) instructionTimerText.text = $"Starting in... {instructionTime:F0}s";
        //        yield return null;
        //    }
        //}

       // instructionPanel.SetActive(false);
        minigamePanel.SetActive(true);
        rolled = false;
        Result = MinigameManager.ResultType.Fail;

        while (!rolled)
            yield return null;

        yield return new WaitForSecondsRealtime(1f);
    }
}
