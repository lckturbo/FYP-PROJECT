using System.Collections;
using TMPro;
using UnityEngine;

public class MinigameTutrol : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject minigamePanel;

    [Header("Countdown UI")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private float countdownDuration = 3f;

    [Header("Minigame References (only one should be active)")]
    [SerializeField] private Wordle wordleScript;
    [SerializeField] private TypingSpeedTest typingGameScript;
    [SerializeField] private Scramble scrambleScript;
    [SerializeField] private ScrabbleGame scrabbleScript;
    [SerializeField] private HangMan hangmanScript;

    private bool countdownDone = false;

    void Start()
    {
        if (tutorialPanel) tutorialPanel.SetActive(true);
        if (minigamePanel) minigamePanel.SetActive(false);

        if (countdownText)
            countdownText.text = "";

        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        float remaining = countdownDuration;

        // --- Countdown phase ---
        while (remaining > 0)
        {
            if (countdownText)
                countdownText.text = Mathf.Ceil(remaining).ToString();
            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }

        countdownDone = true;

        if (countdownText)
            countdownText.text = "GO!";

        //yield return new WaitForSeconds(0.5f);

        if (tutorialPanel) tutorialPanel.SetActive(false);
        if (minigamePanel) minigamePanel.SetActive(true);

        // === Start whichever minigame is assigned ===
        if (wordleScript)
            wordleScript.BeginMinigame();

        if (typingGameScript)
            typingGameScript.BeginMinigame();

        if (scrambleScript)
            scrambleScript.BeginMinigame();

        if (scrabbleScript)
            scrabbleScript.BeginMinigame();

        if (hangmanScript)
            hangmanScript.BeginMinigame();

        if (countdownText)
            countdownText.gameObject.SetActive(false);
    }
}
