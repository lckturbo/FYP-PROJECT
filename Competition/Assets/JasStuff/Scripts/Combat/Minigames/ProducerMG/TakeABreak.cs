using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TakeABreak : BaseMinigame
{
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

        //TurnEngine engine = FindObjectOfType<TurnEngine>();
        //if (engine) engine.Pause(false);

        rolled = true;
    }
    public override IEnumerator Run()
    {
        rolled = false;
        Result = MinigameManager.ResultType.Fail;

        while (!rolled)
            yield return null;

        yield return new WaitForSecondsRealtime(1f);
    }
}
