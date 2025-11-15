using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RedLightGreenLightMinigame : BaseMinigame
{
    [SerializeField] private GameObject animPanel;
    [SerializeField] private Animator anim;

    [Header("Minigame UIs")]
    [SerializeField] private RectTransform starter;
    [SerializeField] private RectTransform player;
    [SerializeField] private RectTransform finishLine;
    [SerializeField] private Image lightIndicator;

    [Header("Texts")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text durationText;

    [Header("Settings")]
    [SerializeField] private float minigameDuration;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float warningDuration;
    private bool isGreenLight;

    private bool running = false;
    private float timer;

    private void Update()
    {
        if (!running) return;

        if (Input.GetKey(KeyCode.Space))
        {
            if (isGreenLight)
                player.anchoredPosition -= Vector2.right * moveSpeed * Time.unscaledDeltaTime;
            else
            {
                running = false;
                // lose minigame 
                Result = MinigameManager.ResultType.Fail;
                return;
            }
        }

        if (player.anchoredPosition.x <= finishLine.anchoredPosition.x)
        {
            running = false;

            Result = timer > minigameDuration * 0.5f ? // 50% left -> 12.5s = perfect, else = success
                  MinigameManager.ResultType.Perfect
                : MinigameManager.ResultType.Success;

            Debug.Log($"Success - Result: {Result}");
        }
    }
    public override IEnumerator Run()
    {
        BattleManager.instance?.SetBattlePaused(true);
        SetupSkipUI(true);

        if (anim)
        {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            anim.SetTrigger("start");
            yield return new WaitForSecondsRealtime(6.2f);
        }

        animPanel.SetActive(false);
        instructionPanel.SetActive(true);
        minigamePanel.SetActive(false);

        while (instructionTime > 0 && !skipRequested)
        {
            instructionTime -= Time.unscaledDeltaTime;
            if (instructionTimerText) instructionTimerText.text = $"Starting in... {instructionTime:F0}s";
            yield return null;
        }

        if (skipRequested)
            instructionTime = 0f;

        instructionPanel.SetActive(false);
        minigamePanel.SetActive(true);
        SetupSkipUI(false);

        yield return StartCoroutine(RunMinigame());

        statusText.text = Result.ToString();
        yield return new WaitForSecondsRealtime(1.0f);

        BattleManager.instance?.SetBattlePaused(false);
    }

    private IEnumerator RunMinigame()
    {
        timer = minigameDuration;
        running = true;

        while (running && timer > 0f)
        {
            bool nextGreen = !isGreenLight;
            if (!nextGreen && isGreenLight)
                yield return StartCoroutine(ShowWarning());

            SetLight(nextGreen);
            float waitTime = isGreenLight
                 ? Random.Range(1.5f, 3f) // green light
                 : Random.Range(1f, 2.5f); // red light

            float elapsed = 0f;
            while (elapsed < waitTime)
            {
                if (!running) yield break;

                timer -= Time.unscaledDeltaTime;
                durationText.text = timer.ToString("F2");
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        if (!running || timer < 0f)
        {
            running = false;
            Result = MinigameManager.ResultType.Fail;
        }
        yield return null;
    }
    private void SetLight(bool green)
    {
        isGreenLight = green;
        starter.rotation = Quaternion.Euler(0, green ? 0 : 180, 0);
        statusText.text = green ? "GREEN LIGHT" : "RED LIGHT";
        lightIndicator.color = green ? Color.green : Color.red;
    }

    private IEnumerator ShowWarning()
    {
        float elapsed = 0f;
        statusText.text = "....";

        while (elapsed < warningDuration)
        {
            timer -= Time.unscaledDeltaTime;
            durationText.text = timer.ToString("F2");
            if (timer <= 0f)
            {
                running = false;
                yield break;
            }

            lightIndicator.color = (Mathf.FloorToInt(elapsed * 10) % 2 == 0)
                ? Color.green
                : Color.yellow;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}
