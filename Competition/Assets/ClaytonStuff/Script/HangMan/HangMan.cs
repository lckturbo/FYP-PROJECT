using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HangMan : BaseMinigame
{
    [Header("UI References")]
    public TMP_Text wordDisplayText;
    public TMP_Text wrongLettersText;
    public TMP_Text messageText;
    public TMP_Text playerInputText;
    public TMP_Text timerText;

    [Header("Hangman Drawing Parts")]
    public List<GameObject> hangmanParts;

    [Header("Animator")]
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject animationPanel;
    [SerializeField] private GameObject tutoralPanel;

    [Header("Game Settings")]
    public string secretWord = "UNITY";
    public int maxAttempts = 6;
    public float attemptTime = 10f;
    public int maxPoints = 100;

    private List<char> guessedLetters = new List<char>();
    private List<char> wrongLetters = new List<char>();
    private string currentInput = "";
    private int remainingAttempts;
    private float timer;
    private bool gameOver = false;
    private int totalMistakes = 0; // <-- NEW: includes wrong guesses + timeouts

    private bool canStart = false;


    public void BeginMinigame()
    {
        canStart = true;
        // Initialize the game only when countdown ends
        secretWord = secretWord.ToUpper();
        remainingAttempts = maxAttempts;
        timer = attemptTime;

        foreach (var part in hangmanParts)
            part.SetActive(false);

        guessedLetters.Clear();
        wrongLetters.Clear();
        totalMistakes = 0;
        gameOver = false;

        UpdateWordDisplay();
        playerInputText.text = "";
        messageText.text = "";
        UpdateTimerUI();
    }


    void Update()
    {
        if (!canStart || gameOver) return;

        HandleTyping();
        HandleTimer();
    }

    void HandleTyping()
    {
        foreach (char c in Input.inputString)
        {
            AudioManager.instance.PlaySFXAtPoint("keyboard-typing-one-short-1-292590", transform.position);
            if (char.IsLetter(c))
            {
                if (currentInput.Length < 1)
                {
                    currentInput = char.ToUpper(c).ToString();
                    playerInputText.text = currentInput;
                }
            }
            else if (c == '\b')
            {
                currentInput = "";
                playerInputText.text = currentInput;
            }
            else if (c == '\n' || c == '\r')
            {
                if (!string.IsNullOrEmpty(currentInput))
                {
                    SubmitGuess(currentInput[0]);
                    currentInput = "";
                    playerInputText.text = "";
                    timer = attemptTime; // Reset timer
                }
            }
        }
    }

    void HandleTimer()
    {
        timer -= Time.unscaledDeltaTime;
        UpdateTimerUI();

        if (timer <= 0f)
        {
            messageText.text = "Time's up! You missed a guess.";
            remainingAttempts--;
            totalMistakes++; // <-- Count timeout as mistake
            UpdateHangmanDrawing();
            currentInput = "";
            playerInputText.text = "";
            timer = attemptTime;
            CheckGameStatus();
        }
    }

    void SubmitGuess(char guessedChar)
    {
        if (guessedLetters.Contains(guessedChar) || wrongLetters.Contains(guessedChar))
        {
            messageText.text = "Already guessed that!";
            return;
        }

        if (secretWord.Contains(guessedChar.ToString()))
        {
            guessedLetters.Add(guessedChar);
            messageText.text = "Good guess!";
        }
        else
        {
            AudioManager.instance.PlaySFXAtPoint("shock-gasp-female-383751", transform.position);
            wrongLetters.Add(guessedChar);
            remainingAttempts--;
            totalMistakes++; // <-- Also increment here
            messageText.text = $"Wrong! {remainingAttempts} tries left.";
            UpdateHangmanDrawing();
        }

        UpdateWordDisplay();
        CheckGameStatus();
    }

    void UpdateWordDisplay()
    {
        string display = "";
        foreach (char c in secretWord)
            display += guessedLetters.Contains(c) ? c + " " : "_ ";
        wordDisplayText.text = display.Trim();

        wrongLettersText.text = "Wrong: " + string.Join(", ", wrongLetters);
    }

    void UpdateHangmanDrawing()
    {
        for (int i = 0; i < hangmanParts.Count; i++)
            hangmanParts[i].SetActive(i < totalMistakes); // <-- use totalMistakes
    }

    void UpdateTimerUI()
    {
        timerText.text = $"{timer:F1}s";
    }

    void CheckGameStatus()
    {
        bool allRevealed = true;
        foreach (char c in secretWord)
        {
            if (!guessedLetters.Contains(c))
            {
                allRevealed = false;
                break;
            }
        }

        if (allRevealed)
        {
            gameOver = true;
            int points = Mathf.Max(0, maxPoints - (totalMistakes * (maxPoints / maxAttempts)));
            points = Mathf.FloorToInt(points * (timer / attemptTime));
            messageText.text = $"You Win! Points: {points}";

            if (points >= maxPoints * 0.6f)
                Result = MinigameManager.ResultType.Perfect;
            else if (points >= maxPoints * 0.4f)
                Result = MinigameManager.ResultType.Success;
            else
                Result = MinigameManager.ResultType.Fail;
        }
        else if (remainingAttempts <= 0)
        {
            gameOver = true;
            messageText.text = $"Game Over! The word was: {secretWord}. Points: 0";
            Result = MinigameManager.ResultType.Fail;
        }
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

        while (!gameOver)
            yield return null;
        BattleManager.instance?.SetBattlePaused(false);
    }
}
