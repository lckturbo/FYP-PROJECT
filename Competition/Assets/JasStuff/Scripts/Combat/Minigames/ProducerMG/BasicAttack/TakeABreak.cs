using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TakeABreak : BaseMinigame
{
    [SerializeField] private GameObject animPanel;
    [SerializeField] private Animator anim;

    [Header("Dice")]
    [SerializeField] private Image diceImage;
    [SerializeField] private Sprite[] diceSides;
    [SerializeField] private int rollAnimationSpeed;
    [SerializeField] private float rollAnimationDuration;

    [SerializeField] private Button rollButton;
    [SerializeField] private TMP_Text resultText;

    private bool rolled;

    protected override void Start()
    {
        base.Start();
        rollButton.onClick.AddListener(OnRoll);
        resultText.text = "Press ROLL!";
    }
    private void OnRoll()
    {
        rollButton.interactable = false;
        resultText.gameObject.SetActive(false);
        diceImage.gameObject.SetActive(true);

        StartCoroutine(RollDiceRoutine());
    }

    private IEnumerator RollDiceRoutine()
    {
        float timer = 0f;

        while (timer < rollAnimationDuration)
        {
            diceImage.sprite = diceSides[Random.Range(0, diceSides.Length)];
            timer += Time.unscaledDeltaTime;
            yield return new WaitForSecondsRealtime(1f / rollAnimationSpeed);
        }

        int final = Random.Range(1, 7);
        diceImage.sprite = diceSides[final - 1];

        if (final == 6)
            Result = MinigameManager.ResultType.Perfect;
        else if (final >= 4)
            Result = MinigameManager.ResultType.Success;
        else
            Result = MinigameManager.ResultType.Fail;

        resultText.gameObject.SetActive(true);
        resultText.text = Result.ToString();
        rolled = true;
    }

    public override IEnumerator Run()
    {
        BattleManager.instance?.SetBattlePaused(true);
        if (anim)
        {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            anim.SetTrigger("start");
            yield return new WaitForSecondsRealtime(3.5f);
        }

        animPanel.SetActive(false);
        minigamePanel.SetActive(true);

        rolled = false;
        Result = MinigameManager.ResultType.Fail;

        while (!rolled)
            yield return null;

        yield return new WaitForSecondsRealtime(1f);
        BattleManager.instance?.SetBattlePaused(false);
    }
}
