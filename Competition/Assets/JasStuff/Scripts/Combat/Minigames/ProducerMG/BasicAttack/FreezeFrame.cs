using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FreezeFrame : BaseMinigame
{
    [SerializeField] private GameObject animPanel;
    [SerializeField] private Animator anim;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text scoreText;

    [Header("References")]
    [SerializeField] private RectTransform markerParent;
    [SerializeField] private RectTransform markerRect;

    [SerializeField] private RectTransform[] targetZones;
    [SerializeField] private Image markerImage;
    [SerializeField] private Image[] targetImage;
    [SerializeField] private Outline[] targetOutlines;
    [SerializeField] private Sprite[] poseSprites;

    [SerializeField] private CanvasGroup minigamePanelGroup;
    [SerializeField] private RectTransform scoreTextRect;
    [SerializeField] private TMP_Text resultText;

    [Header("Gameplay Settings")]
    [SerializeField] private float hitTolerance = 35f;
    [SerializeField] private float moveSpeed = 300f;

    private int score = 0;
    private float timer;
    private bool running = false;
    private int correctIndex;
    private Sprite currentCorrectPose;
    private RectTransform correctTargetZone;
    private bool canHit = true;

    public override IEnumerator Run()
    {
        BattleManager.instance?.SetBattlePaused(true);
        if (anim)
        {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            anim.SetTrigger("start");
            yield return new WaitForSecondsRealtime(4.2f);
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

        timer = 15.0f;
        running = true;

        SetNewRound();
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

        if (score >= 7)
            Result = MinigameManager.ResultType.Perfect;
        else if (score >= 3)
            Result = MinigameManager.ResultType.Success;
        else
            Result = MinigameManager.ResultType.Fail;

        Debug.Log($"Minigame ended with result: {Result}");
        yield return StartCoroutine(EndMinigameSequence());

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
        if (!canHit) return; 
        canHit = false;

        RectTransform hitZone = GetClosestZone();
        int hitZoneIndex = hitZone.GetSiblingIndex();

        float markerX = markerRect.anchoredPosition.x;
        float targetX = hitZone.anchoredPosition.x;
        bool isInZone = Mathf.Abs(markerX - targetX) <= hitTolerance;

        bool poseMatch = (markerImage.sprite == currentCorrectPose);

        bool isCorrectZone = (hitZone == correctTargetZone);

        if (isInZone && poseMatch && isCorrectZone)
        {
            score++;
            Debug.Log($"Correct! Score: {score}");
            SetScoreText(score.ToString());
            StartCoroutine(FlashBorder(targetOutlines[hitZoneIndex], Color.green));
        }
        else
        {
            Debug.Log("Wrong!");
            StartCoroutine(FlashBorder(targetOutlines[hitZoneIndex], Color.red));
        }
    }

    private IEnumerator FlashBorder(Outline outline, Color color)
    {
        Color original = outline.effectColor;
        outline.effectColor = color;

        yield return new WaitForSecondsRealtime(0.5f);

        outline.effectColor = original;
        SetNewRound();
        canHit = true;
    }
    private void SetScoreText(string text)
    {
        if (scoreText) scoreText.text = "Score: " + text;
    }
    private RectTransform GetClosestZone()
    {
        RectTransform closest = null;
        float closestDist = float.MaxValue;
        float markerX = markerRect.anchoredPosition.x;

        foreach (var zone in targetZones)
        {
            float dist = Mathf.Abs(markerX - zone.anchoredPosition.x);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = zone;
            }
        }

        return closest;
    }
    private void SetNewRound()
    {
        currentCorrectPose = poseSprites[Random.Range(0, poseSprites.Length)];
        markerImage.sprite = currentCorrectPose;

        Sprite wrong1, wrong2;
        do { wrong1 = poseSprites[Random.Range(0, poseSprites.Length)]; }
        while (wrong1 == currentCorrectPose);

        do { wrong2 = poseSprites[Random.Range(0, poseSprites.Length)]; }
        while (wrong2 == currentCorrectPose || wrong2 == wrong1);

        Sprite[] set = new Sprite[] { currentCorrectPose, wrong1, wrong2 };
        set = Shuffle(set);

        targetImage[0].sprite = set[0];
        targetImage[1].sprite = set[1];
        targetImage[2].sprite = set[2];

        for (int i = 0; i < targetImage.Length; i++)
            if (targetImage[i].sprite == currentCorrectPose)
                correctTargetZone = targetImage[i].rectTransform;
    }
    private Sprite[] Shuffle(Sprite[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int rand = Random.Range(i, array.Length);
            (array[i], array[rand]) = (array[rand], array[i]);
        }
        return array;
    }

    private IEnumerator EndMinigameSequence()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 1.5f;
            minigamePanelGroup.alpha = 1f - t;
            yield return null;
        }

        Vector2 startPos = scoreTextRect.anchoredPosition;
        Vector2 endPos = Vector2.zero;

        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 1.2f;
            scoreTextRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        resultText.gameObject.SetActive(true);
        resultText.text = Result.ToString().ToUpper();

        yield return new WaitForSecondsRealtime(1.5f);
    }


}
