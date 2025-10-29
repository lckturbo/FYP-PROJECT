using System.Collections;
using TMPro;
using UnityEngine;

public class FreezeFrame : BaseMinigame
{
    [Header("Gameplay Settings")]
    [SerializeField] private GameObject animPanel;
    [SerializeField] private Animator anim;
    [SerializeField] private TMP_Text timerText;

    [SerializeField] private RectTransform markerParent;
    [SerializeField] private RectTransform markerRect;
    [SerializeField] private RectTransform targetZone;

    [SerializeField] private float hitTolerance = 35f;
    [SerializeField] private float moveSpeed = 300f;

    private int score = 0;
    private float timer;
    private bool running = false;

    public override IEnumerator Run()
    {
        BattleManager.instance?.SetBattlePaused(true);
        if (anim)
        {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            anim.SetTrigger("start");
            yield return new WaitForSecondsRealtime(4.5f);
        }

        animPanel.SetActive(false);
        instructionPanel.SetActive(true);
        minigamePanel.SetActive(false);

        if (instructionPanel.activeSelf)
        {
            while (instructionTime > 0)
            {
                instructionTime -= Time.unscaledDeltaTime;
                if (instructionTimerText) instructionTimerText.text = $"Starting in... {instructionTime:F0}s";
                yield return null;
            }
        }

        instructionPanel.SetActive(false);
        minigamePanel.SetActive(true);

        timer = 10.0f;
        running = true;

        StartCoroutine(MoveMarker());

        while (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;
            if (timerText) timerText.text = $"{timer:F1}s";

            if (Input.GetKeyDown(KeyCode.Space))
                CheckHit();

            yield return null;
        }

        running = false;

        if (score >= 5)
            Result = MinigameManager.ResultType.Perfect;
        else if (score >= 2)
            Result = MinigameManager.ResultType.Success;
        else
            Result = MinigameManager.ResultType.Fail;

        Debug.Log($"Minigame ended with result: {Result}");

        BattleManager.instance?.SetBattlePaused(false);
    }
    private IEnumerator MoveMarker()
    {
        float leftX = -markerParent.rect.width / 2f + markerRect.rect.width / 2f;
        float rightX = markerParent.rect.width / 2f - markerRect.rect.width / 2f;

        float targetX = rightX; 

        while (running)
        {
            markerRect.anchoredPosition = Vector2.MoveTowards(
                markerRect.anchoredPosition,
                new Vector2(targetX, 0f),
                moveSpeed * Time.unscaledDeltaTime
            );

            if (Mathf.Abs(markerRect.anchoredPosition.x - targetX) < 1f)
                targetX = (targetX == rightX) ? leftX : rightX;

            yield return null;
        }
    }

    private void CheckHit()
    {
        float markerX = markerRect.anchoredPosition.x;
        float targetX = targetZone.anchoredPosition.x;

        if (Mathf.Abs(markerX - targetX) <= hitTolerance)
        {
            score++;
            Debug.Log($"Hit! Score: {score}");
        }
        else
        {
            Debug.Log("Miss!");
        }
    }

}
