using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HangMan : BaseMinigame
{
    [Header("UI References")]
    public TMP_Text wordDisplayText;       // Shows current word progress
    public TMP_Text wrongLettersText;      // Shows wrong guesses
    public TMP_Text messageText;           // Feedback / win-loss messages
    public TMP_Text playerInputText;       // Displays letters as player types
    public TMP_Text timerText;             // Countdown timer

    [Header("Hangman Drawing Parts")]
    public List<GameObject> hangmanParts;

    [Header("Game Settings")]
    public string secretWord = "UNITY";
    public int maxAttempts = 6;
    public float attemptTime = 10f;        // Time per guess
    public int maxPoints = 100;

    private List<char> guessedLetters = new List<char>();
    private List<char> wrongLetters = new List<char>();
    private string currentInput = "";
    private int remainingAttempts;
    private float timer;
    private bool gameOver = false;

    void Start()
    {
        secretWord = secretWord.ToUpper();
        remainingAttempts = maxAttempts;
        timer = attemptTime;

        foreach (var part in hangmanParts)
            part.SetActive(false);

        UpdateWordDisplay();
        playerInputText.text = "";
        messageText.text = "";
        UpdateTimerUI();
    }

    void Update()
    {
        if (gameOver) return;

        HandleTyping();
        HandleTimer();
    }

    void HandleTyping()
    {
        foreach (char c in Input.inputString)
        {
            if (char.IsLetter(c))
            {
                if (currentInput.Length < 1) // Only allow one letter at a time
                {
                    currentInput = char.ToUpper(c).ToString();
                    playerInputText.text = currentInput;
                }
            }
            else if (c == '\b') // Backspace
            {
                currentInput = "";
                playerInputText.text = currentInput;
            }
            else if (c == '\n' || c == '\r') // Enter
            {
                if (!string.IsNullOrEmpty(currentInput))
                {
                    SubmitGuess(currentInput[0]);
                    currentInput = "";
                    playerInputText.text = "";
                    timer = attemptTime; // Reset timer for next letter
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
            messageText.text = "? Time's up! Moving to next attempt.";
            remainingAttempts--;
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
            wrongLetters.Add(guessedChar);
            remainingAttempts--;
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
        int wrongCount = wrongLetters.Count;
        for (int i = 0; i < hangmanParts.Count; i++)
            hangmanParts[i].SetActive(i < wrongCount);
    }

    void UpdateTimerUI()
    {
        timerText.text = $"? {timer:F1}s";
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

            // Calculate points like Scramble
            int points = Mathf.Max(0, maxPoints - (wrongLetters.Count * (maxPoints / maxAttempts)));
            points = Mathf.FloorToInt(points * (timer / attemptTime)); // Bonus for faster guesses

            messageText.text = $"? You Win! Points: {points}";

            // Determine ResultType
            if (points >= maxPoints * 0.8f)
                Result = MinigameManager.ResultType.Perfect;
            else if (points >= maxPoints * 0.5f)
                Result = MinigameManager.ResultType.Success;
            else
                Result = MinigameManager.ResultType.Fail;
        }
        else if (remainingAttempts <= 0)
        {
            gameOver = true;
            messageText.text = $"? Game Over! The word was: {secretWord}. Points: 0";
            Result = MinigameManager.ResultType.Fail;
        }
    }


    public override IEnumerator Run()
    {
        BattleManager.instance?.SetBattlePaused(true);

        while (!gameOver)
            yield return null;

        BattleManager.instance?.SetBattlePaused(false);
    }
}
