using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Scramble : BaseMinigame
{
    [Header("UI References")]
    [SerializeField] private TMP_Text scrambledWordText;   // Shows scrambled word
    [SerializeField] private TMP_Text playerGuessText;     // Shows the current typed input
    [SerializeField] private TMP_Text messageText;         // Feedback messages
    [SerializeField] private TMP_Text attemptText;         // Shows remaining attempts
    [SerializeField] private TMP_Text timerText;           // Shows countdown timer

    [Header("Game Settings")]
    [SerializeField] private string[] wordList = { "UNITY", "SCRIPT", "PLANET", "GAMES", "CLOUD", "EDITOR" };
    [SerializeField] private int maxAttempts = 3;
    [SerializeField] private int maxPoints = 100;
    [SerializeField] private float maxTime = 10f;          // 10-second timer

    private string targetWord;
    private string scrambledWord;
    private string currentInput = "";
    private int remainingAttempts;
    private bool gameOver = false;
    private float timeLeft;

    void Start()
    {
        StartNewGame();
    }

    void StartNewGame()
    {
        // Pick a random word
        targetWord = wordList[Random.Range(0, wordList.Length)].ToUpper();
        scrambledWord = ScrambleWord(targetWord);
        remainingAttempts = maxAttempts;
        timeLeft = maxTime;
        gameOver = false;
        currentInput = "";

        // Update UI
        scrambledWordText.text = scrambledWord;
        playerGuessText.text = "";
        messageText.text = "Type and press Enter to guess!";
        attemptText.text = $"Attempts Left: {remainingAttempts}";
        timerText.text = $"Time: {timeLeft:F1}s";

        Debug.Log($"[Scramble] Target Word: {targetWord}");
    }

    void Update()
    {
        if (gameOver) return;

        // Countdown timer
        timeLeft -= Time.unscaledDeltaTime;
        timerText.text = $"Time: {timeLeft:F1}s";
        if (timeLeft <= 0f)
        {
            messageText.text = $" Time’s up! The word was {targetWord}.";
            EndGame(0);
            return;
        }

        // Detect key input
        foreach (char c in Input.inputString)
        {
            if (c == '\b') // Backspace
            {
                if (currentInput.Length > 0)
                    currentInput = currentInput.Substring(0, currentInput.Length - 1);
            }
            else if (c == '\n' || c == '\r') // Enter key
            {
                OnSubmitGuess();
                return;
            }
            else if (char.IsLetter(c))
            {
                currentInput += char.ToUpper(c);
            }
        }

        playerGuessText.text = currentInput;
    }

    string ScrambleWord(string word)
    {
        char[] chars = word.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            int randIndex = Random.Range(0, chars.Length);
            (chars[i], chars[randIndex]) = (chars[randIndex], chars[i]);
        }

        // Prevent showing the same unscrambled word
        string result = new string(chars);
        if (result == word) return ScrambleWord(word);
        return result;
    }

    void OnSubmitGuess()
    {
        if (gameOver) return;

        string guess = currentInput.ToUpper().Trim();
        currentInput = "";

        if (string.IsNullOrEmpty(guess))
        {
            messageText.text = " Enter a guess!";
            return;
        }

        if (guess == targetWord)
        {
            int points = Mathf.Max(0, maxPoints - ((maxAttempts - remainingAttempts) * (maxPoints / maxAttempts)));
            points = Mathf.FloorToInt(points * (timeLeft / maxTime)); // Bonus for faster answers
            messageText.text = $" Correct! The word was {targetWord}\nYou scored {points} points!";
            EndGame(points);
        }
        else
        {
            remainingAttempts--;
            attemptText.text = $"Attempts Left: {remainingAttempts}";
            messageText.text = $" Wrong guess! Try again.";

            if (remainingAttempts <= 0)
            {
                messageText.text = $" Out of attempts! The word was {targetWord}.";
                EndGame(0);
            }
        }

        playerGuessText.text = "";
    }

    void EndGame(int points)
    {
        gameOver = true;

        if (points >= maxPoints * 0.8f)
            Result = MinigameManager.ResultType.Perfect;
        else if (points >= maxPoints * 0.5f)
            Result = MinigameManager.ResultType.Success;
        else
            Result = MinigameManager.ResultType.Fail;
    }

    public override IEnumerator Run()
    {
        BattleManager.instance?.SetBattlePaused(true);
        
        while (!gameOver)
            yield return null;

        BattleManager.instance?.SetBattlePaused(false);
    }
}
