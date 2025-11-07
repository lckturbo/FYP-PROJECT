using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FreezeFrame : BaseMinigame
{
    [SerializeField] private GameObject animPanel;
    [SerializeField] private Animator anim;
    [SerializeField] private TMP_Text timerText;

    [Header("References")]
    [SerializeField] private RectTransform markerParent;
    [SerializeField] private RectTransform markerRect;
    [SerializeField] private RectTransform targetZone;
    [SerializeField] private Image markerImage;
    [SerializeField] private Image[] targetImage;
    [SerializeField] private Sprite[] poseSprites;

    [Header("Gameplay Settings")]
    [SerializeField] private float hitTolerance = 35f;
    [SerializeField] private float moveSpeed = 300f;

    private int score = 0;
    private float timer;
    private bool running = false;
    private int correctIndex;
    private Sprite currentCorrectPose;
    private RectTransform correctTargetZone;

    private void Start()
    {
        StartCoroutine(Run());
    }
    private void SetNewRound()
    {
        // pick a random sprite as the correct answer
        currentCorrectPose = poseSprites[Random.Range(0, poseSprites.Length)];
        markerImage.sprite = currentCorrectPose;

        // pick 2 incorrect sprites
        Sprite wrong1, wrong2;
        do { wrong1 = poseSprites[Random.Range(0, poseSprites.Length)]; }
        while (wrong1 == currentCorrectPose);

        do { wrong2 = poseSprites[Random.Range(0, poseSprites.Length)]; }
        while (wrong2 == currentCorrectPose || wrong2 == wrong1);

        // place them randomly in the 3 target zones
        Sprite[] set = new Sprite[] { currentCorrectPose, wrong1, wrong2 };
        set = Shuffle(set);

        targetImage[0].sprite = set[0];
        targetImage[1].sprite = set[1];
        targetImage[2].sprite = set[2];

        // assign the correct target zone index
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

    public override IEnumerator Run()
    {
        //BattleManager.instance?.SetBattlePaused(true);
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

        timer = 10.0f;
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

        if (score >= 5)
            Result = MinigameManager.ResultType.Perfect;
        else if (score >= 2)
            Result = MinigameManager.ResultType.Success;
        else
            Result = MinigameManager.ResultType.Fail;

        Debug.Log($"Minigame ended with result: {Result}");

       // BattleManager.instance?.SetBattlePaused(false);
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
        // check horizontal alignment with selected correct zone
        float markerX = markerRect.anchoredPosition.x;
        float targetX = correctTargetZone.anchoredPosition.x;
        bool isInZone = Mathf.Abs(markerX - targetX) <= hitTolerance;

        // check pose match
        bool poseMatch = (markerImage.sprite == currentCorrectPose);

        if (isInZone && poseMatch)
        {
            score++;
            Debug.Log($"Correct! Score: {score}");
        }
        else
        {
            Debug.Log("Wrong!");
        }

        SetNewRound(); // Go to next round
    }


}
