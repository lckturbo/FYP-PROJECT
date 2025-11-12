using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinigameTutrol : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject minigamePanel;

    [Header("Countdown UI")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private float countdownDuration = 3f;

    [Header("Skip Button")]
    [SerializeField] private Button skipButton;

    [Header("Minigame References (only one should be active)")]
    [SerializeField] private Wordle wordleScript;
    [SerializeField] private TypingSpeedTest typingGameScript;
    [SerializeField] private Scramble scrambleScript;
    [SerializeField] private ScrabbleGame scrabbleScript;
    [SerializeField] private HangMan hangmanScript;

    private bool countdownDone = false;
    private Coroutine countdownRoutine;

    void Start()
    {
        if (tutorialPanel) tutorialPanel.SetActive(false);
        if (minigamePanel) minigamePanel.SetActive(false);

        if (countdownText)
            countdownText.text = "";

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(SkipTutorial);
        }

        countdownRoutine = StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        float remaining = countdownDuration;

        // --- Countdown phase ---
        while (remaining > 0 && !countdownDone)
        {
            if (countdownText)
                countdownText.text = Mathf.Ceil(remaining).ToString();
            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }

        StartMinigame();
    }

    private void SkipTutorial()
    {
        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);

        StartMinigame();
    }

    private void StartMinigame()
    {
        if (countdownDone) return;
        countdownDone = true;

        if (tutorialPanel) tutorialPanel.SetActive(false);
        if (minigamePanel) minigamePanel.SetActive(true);
        if (countdownText) countdownText.gameObject.SetActive(false);
        if (skipButton) skipButton.gameObject.SetActive(false);

        // === Start whichever minigame is assigned ===
        if (wordleScript) wordleScript.BeginMinigame();
        if (typingGameScript) typingGameScript.BeginMinigame();
        if (scrambleScript) scrambleScript.BeginMinigame();
        if (scrabbleScript) scrabbleScript.BeginMinigame();
        if (hangmanScript) hangmanScript.BeginMinigame();
    }
}
