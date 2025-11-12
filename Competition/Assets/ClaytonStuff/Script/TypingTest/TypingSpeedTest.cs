using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class TypingSpeedTest : BaseMinigame
{
    [Header("UI References")]
    [SerializeField] private TMP_Text typingText; // Single text like Monkeytype
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text timerText;

    [Header("Sentences")]
    [TextArea(2, 5)]
    public List<string> sentences = new List<string>
    {
        "The quick brown fox jumps over the lazy dog.",
        "Unity makes game development easier and faster.",
        "Typing fast and accurately takes practice.",
        "Clean code is better than clever code."
    };

    [Header("Animator")]
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject animationPanel;
    [SerializeField] private GameObject tutoralPanel;

    private string currentSentence = "";
    private string playerInput = "";
    private float startTime;
    private bool testActive = false;
    private bool gameOver = false;
    [SerializeField] private float maxTime = 30f; // Longer test time for Monkeytype feel

    void Start()
    {
       //StartNewTest();
    }
    public void BeginMinigame()
    {
        // Reset the test state
        StartNewTest();
    }

    void Update()
    {
        if (!testActive) return;

        // --- Correct countdown using unscaledTime ---
        float timeLeft = Mathf.Max(maxTime - (Time.unscaledTime - startTime), 0f);
        timerText.text = $"{timeLeft:F1}s";

        if (timeLeft <= 0f)
        {
            EndTest();
            return;
        }

        foreach (char c in Input.inputString)
        {
            if (c == '\b')
            {
                if (playerInput.Length > 0)
                    playerInput = playerInput.Substring(0, playerInput.Length - 1);
            }
            else if (c == '\n' || c == '\r')
            {
                EndTest();
                return;
            }
            else
            {
                playerInput += c;
            }

            UpdateTypingDisplay();

            if (playerInput.Length >= currentSentence.Length)
            {
                EndTest();
                return;
            }
        }
    }

    private void UpdateTypingDisplay()
    {
        string formatted = "";

        for (int i = 0; i < currentSentence.Length; i++)
        {
            if (i < playerInput.Length)
            {
                if (playerInput[i] == currentSentence[i])
                    formatted += $"<color=#000000>{currentSentence[i]}</color>"; // Correct - black
                else
                    formatted += $"<color=#FF5555>{currentSentence[i]}</color>"; // Incorrect - red
            }
            else if (i == playerInput.Length)
            {
                formatted += $"<mark=#FFFF0055>{currentSentence[i]}</mark>"; // Caret highlight - yellow
            }
            else
            {
                formatted += $"<color=#D9D9D9>{currentSentence[i]}</color>"; // Future - faint gray
            }
        }

        typingText.text = formatted;
    }

    void StartNewTest()
    {
        resultText.text = "";
        playerInput = "";
        currentSentence = sentences[Random.Range(0, sentences.Count)];

        startTime = Time.unscaledTime;
        testActive = true;
        gameOver = false;

        UpdateTypingDisplay();
        timerText.text = $"{maxTime:F1}s";
    }

    private void EndTest()
    {
        testActive = false;
        float timeTaken = Mathf.Min(Time.unscaledTime - startTime, maxTime);

        // --- Word-based scoring ---
        string[] sentenceWords = currentSentence.Trim().Split(' ');
        string[] inputWords = playerInput.Trim().Split(' ');

        int correctWords = 0;
        for (int i = 0; i < Mathf.Min(sentenceWords.Length, inputWords.Length); i++)
        {
            if (sentenceWords[i].TrimEnd('.', ',', '?', '!') == inputWords[i].TrimEnd('.', ',', '?', '!'))
                correctWords++;
        }

        int totalWords = sentenceWords.Length;
        float accuracy = ((float)correctWords / totalWords) * 100f;
        int score = correctWords;

        resultText.text =
            $"<b>Done!</b>\n" +
            $"Score: {score}/{totalWords}\n" +
            $"Accuracy: {accuracy:F1}%\n" +
            $"Time: {timeTaken:F2}s";

        timerText.text = "0.0s";

        gameOver = true;

        if (score < 1)
            Result = MinigameManager.ResultType.Fail;
        else if (score <= 4)
            Result = MinigameManager.ResultType.Success;
        else
            Result = MinigameManager.ResultType.Perfect;
    }

    public override IEnumerator Run()
    {
        BattleManager.instance?.SetBattlePaused(true);

        animationPanel.SetActive(true);
        tutoralPanel.SetActive(false);

        if (anim)
        {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            anim.SetTrigger("start");
            yield return new WaitForSecondsRealtime(1.5f);
        }

        animationPanel.SetActive(false);
        tutoralPanel.SetActive(true);

        // Wait until the game is over
        while (!gameOver)
            yield return null;

        // Add a short delay before unpausing and closing
        yield return new WaitForSecondsRealtime(2f);

        BattleManager.instance?.SetBattlePaused(false);
    }
}
