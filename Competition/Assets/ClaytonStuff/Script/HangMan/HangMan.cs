using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HangMan : BaseMinigame
{
    [Header("UI References")]
    public TMP_Text wordDisplayText;
    public TMP_Text wrongLettersText;
    public TMP_Text messageText;
    public TMP_InputField inputField;
    public Button guessButton;

    [Header("Hangman Drawing Parts")]
    public List<GameObject> hangmanParts;

    [Header("Game Settings")]
    public string secretWord = "UNITY";
    public int maxAttempts = 6;
    public int maxPoints = 100; // Maximum points for guessing perfectly

    private List<char> guessedLetters = new List<char>();
    private List<char> wrongLetters = new List<char>();
    private int remainingAttempts;
    private bool gameOver = false;

    void Start()
    {
        secretWord = secretWord.ToUpper();
        remainingAttempts = maxAttempts;

        foreach (var part in hangmanParts)
            part.SetActive(false);

        UpdateWordDisplay();
        guessButton.onClick.AddListener(OnGuessSubmitted);
        messageText.text = "";
    }

    void OnGuessSubmitted()
    {
        if (gameOver) return;

        string input = inputField.text.ToUpper();
        inputField.text = "";

        if (string.IsNullOrEmpty(input) || input.Length > 1)
        {
            messageText.text = "Enter one letter!";
            return;
        }

        char guessedChar = input[0];

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
        {
            display += guessedLetters.Contains(c) ? c + " " : "_ ";
        }

        wordDisplayText.text = display.Trim();
        wrongLettersText.text = "Wrong: " + string.Join(", ", wrongLetters);
    }

    void UpdateHangmanDrawing()
    {
        int wrongCount = wrongLetters.Count;
        for (int i = 0; i < hangmanParts.Count; i++)
        {
            hangmanParts[i].SetActive(i < wrongCount);
        }
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
            guessButton.interactable = false;

            int points = Mathf.Max(0, maxPoints - (wrongLetters.Count * (maxPoints / maxAttempts)));
            messageText.text = $"?? You Win! Points: {points}";

            Result = points >= maxPoints * 0.8f ? MinigameManager.ResultType.Perfect :
                     points >= maxPoints * 0.5f ? MinigameManager.ResultType.Success :
                     MinigameManager.ResultType.Fail;

            return;
        }

        if (remainingAttempts <= 0)
        {
            gameOver = true;
            guessButton.interactable = false;
            messageText.text = $"?? Game Over! The word was: {secretWord}. Points: 0";
            Result = MinigameManager.ResultType.Fail;
        }
    }

    public override IEnumerator Run()
    {
        Result = MinigameManager.ResultType.Fail;
        yield return new WaitForSeconds(1);
    }
}
