using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommandBurst : BaseMinigame
{
    [Header("UI References")]
    [SerializeField] private Animator instrAnim;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Slider energyBar;
    [SerializeField] private TMP_Text timerText;

    [Header("Sprite References")]
    [SerializeField] private Transform promptContainer;
    [SerializeField] private Image keyIconPrefab;
    [SerializeField] private Sprite wSprite;
    [SerializeField] private Sprite aSprite;
    [SerializeField] private Sprite sSprite;
    [SerializeField] private Sprite dSprite;
    [SerializeField] private Sprite m1Sprite;
    [SerializeField] private Sprite m2Sprite;
    private Dictionary<KeyCode, Sprite> keySprites;
    private List<Image> currentIcons = new List<Image>();

    [Header("Settings")]
    //[SerializeField] private float timePerPrompt = 1.5f;
    [SerializeField] private int totalPrompts = 5;

    private KeyCode[] comboKeys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.Mouse0, KeyCode.Mouse1 };
    private int successCount = 0;
    private float timer;
    private bool instructionStarted = false;
    public void StartInstructionCountdown()
    {
        instructionStarted = true;
        Debug.Log("instruction started: " + instructionStarted);
    }
    protected override void Start()
    {
        base.Start();
        keySprites = new Dictionary<KeyCode, Sprite>
        {
            { KeyCode.W, wSprite },
            { KeyCode.A, aSprite },
            { KeyCode.S, sSprite },
            { KeyCode.D, dSprite },
            { KeyCode.Mouse0, m1Sprite },
            { KeyCode.Mouse1, m2Sprite }
        };
    }
    public override IEnumerator Run()
    {
        BattleManager.instance.SetBattlePaused(true);
        SetupSkipUI(true);

        instructionPanel.SetActive(true);
        minigamePanel.SetActive(false);

        if (instrAnim)
        {
            instrAnim.SetTrigger("start");
            instructionStarted = false;
            yield return null;
            while (!instructionStarted && !skipRequested) yield return null;
        }

        while (instructionTime > 0f && !skipRequested)
        {
            instructionTime -= Time.unscaledDeltaTime;

            if (instructionTimerText)
                instructionTimerText.text = $"Starting in... {instructionTime:F0}s";

            yield return null;
        }

        if (skipRequested) instructionTime = 0f;

        instructionPanel.SetActive(false);
        minigamePanel.SetActive(true);
        SetupSkipUI(false);

        yield return StartCoroutine(PlaySequence());

        if (successCount == totalPrompts)
            Result = MinigameManager.ResultType.Perfect;
        else if (successCount >= totalPrompts * 0.6f)
            Result = MinigameManager.ResultType.Success;
        else
            Result = MinigameManager.ResultType.Fail;

        promptText.text = Result + "!";
        yield return new WaitForSecondsRealtime(1.0f);

        BattleManager.instance.SetBattlePaused(false);
    }
    private IEnumerator PlaySequence()
    {
        successCount = 0;
        energyBar.value = 0f;

        timer = 10.0f;
        int currentIndex = 0;

        // Generate first combo
        KeyCode[] combo = GenerateCombo();
        DisplayCombo(combo, currentIndex);

        while (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;
            timerText.text = timer.ToString("F1");

            if (currentIndex < combo.Length)
            {
                if (Input.anyKeyDown)
                {
                    if (Input.GetKeyDown(combo[currentIndex]))
                    {
                        successCount++;
                        currentIndex++;
                        energyBar.value = Mathf.Clamp01(energyBar.value + 0.02f);
                        UpdateComboHighlight(currentIndex);
                    }
                    else
                    {
                        energyBar.value = Mathf.Clamp01(energyBar.value - 0.1f);
                        ShakeEnergyBar();
                    }
                }
            }
            else
            {
                yield return new WaitForSecondsRealtime(0.2f);
                combo = GenerateCombo();
                currentIndex = 0;
                DisplayCombo(combo, currentIndex);
            }

            yield return null;
        }

        promptContainer.gameObject.SetActive(false);
        promptText.gameObject.SetActive(true);

        promptText.text = "TIME UP!";
        yield return new WaitForSecondsRealtime(1.0f);
    }
    private KeyCode[] GenerateCombo()
    {
        KeyCode[] combo = new KeyCode[Random.Range(3, 6)];
        for (int i = 0; i < combo.Length; i++)
            combo[i] = comboKeys[Random.Range(0, comboKeys.Length)];
        return combo;
    }

    private void DisplayCombo(KeyCode[] combo, int progress)
    {
        foreach (Transform child in promptContainer)
            Destroy(child.gameObject);
        currentIcons.Clear();

        for (int i = 0; i < combo.Length; i++)
        {
            Image icon = Instantiate(keyIconPrefab, promptContainer);
            if (keySprites.TryGetValue(combo[i], out Sprite sprite))
                icon.sprite = sprite;
            icon.color = (i == 0) ? Color.yellow : Color.white;
            currentIcons.Add(icon);
        }
    }

    private void ShakeEnergyBar()
    {
        StopCoroutine(nameof(ShakeEnergyBarRoutine));
        StartCoroutine(ShakeEnergyBarRoutine());
    }
    private IEnumerator ShakeEnergyBarRoutine()
    {
        Vector3 originalPos = energyBar.transform.localPosition;
        float shakeTime = 0.2f;

        while (shakeTime > 0)
        {
            shakeTime -= Time.unscaledDeltaTime;
            energyBar.transform.localPosition = originalPos + (Vector3)UnityEngine.Random.insideUnitCircle * 5f;
            yield return null;
        }

        energyBar.transform.localPosition = originalPos;
    }
    private void UpdateComboHighlight(int progress)
    {
        for (int i = 0; i < currentIcons.Count; i++)
        {
            if (i < progress)
                currentIcons[i].color = Color.green; // done
            else if (i == progress)
                currentIcons[i].color = Color.yellow; // current
            else
                currentIcons[i].color = Color.white; // upcoming
        }
    }

}