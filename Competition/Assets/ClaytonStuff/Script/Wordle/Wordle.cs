using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Wordle : BaseMinigame
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Transform gridParent; // parent with 6x5 text boxes
    [SerializeField] private GameObject letterBoxPrefab;
    [SerializeField] private TMP_Text resultText;

    [Header("Game Settings")]
    [SerializeField] private int maxAttempts = 6;
    [SerializeField] private string[] wordList = { "APPLE", "CHAIR", "UNITY", "PLANT", "GAMES", "CLOUD" };

    private string targetWord;
    private int currentAttempt = 0;
    private List<List<TMP_Text>> grid = new List<List<TMP_Text>>();

    private bool gameOver = false;
    private int score = 0;

    void Start()
    {
        SetupGrid();
        StartNewGame();
    }

    void SetupGrid()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        grid.Clear();

        for (int i = 0; i < maxAttempts; i++)
        {
            List<TMP_Text> row = new List<TMP_Text>();
            for (int j = 0; j < 5; j++)
            {
                GameObject box = Instantiate(letterBoxPrefab, gridParent);
                TMP_Text letter = box.GetComponentInChildren<TMP_Text>();
                letter.text = "";
                row.Add(letter);
            }
            grid.Add(row);
        }

        resultText.text = "";
    }

    void StartNewGame()
    {
        targetWord = wordList[Random.Range(0, wordList.Length)].ToUpper();
        currentAttempt = 0;
        score = 0;
        gameOver = false;
        inputField.interactable = true;
        resultText.text = "Guess the 5-letter word!";
        inputField.text = "";

        Debug.Log($"[Wordle] Target word: {targetWord}");
    }

    public void OnSubmitGuess()
    {
        if (gameOver || currentAttempt >= maxAttempts)
            return;

        string guess = inputField.text.ToUpper().Trim();
        if (guess.Length != 5)
        {
            resultText.text = " Must be 5 letters!";
            return;
        }

        DisplayGuess(guess);
        currentAttempt++;
        inputField.text = "";

        if (guess == targetWord)
        {
            CalculateScore();
            resultText.text = $" Correct! You win!\nScore: {score}";
            inputField.interactable = false;
            gameOver = true;
            SetResultBasedOnScore();
        }
        else if (currentAttempt >= maxAttempts)
        {
            score = 0;
            resultText.text = $" Out of attempts!\nWord was: {targetWord}\nScore: {score}";
            inputField.interactable = false;
            gameOver = true;
            Result = MinigameManager.ResultType.Fail;
        }
        else
        {
            resultText.text = $"Attempt {currentAttempt}/{maxAttempts}";
        }
    }

    void DisplayGuess(string guess)
    {
        var row = grid[currentAttempt];

        for (int i = 0; i < 5; i++)
        {
            TMP_Text letterText = row[i];
            letterText.text = guess[i].ToString();

            if (guess[i] == targetWord[i])
            {
                letterText.color = Color.green; // correct spot
            }
            else if (targetWord.Contains(guess[i].ToString()))
            {
                letterText.color = Color.yellow; // wrong spot
            }
            else
            {
                letterText.color = Color.gray; // not in word
            }
        }
    }

    void CalculateScore()
    {
        // Higher score for fewer attempts
        // Example: 6 attempts = 10 pts, 1 attempt = 60 pts
        int baseScore = 60;
        int deduction = (currentAttempt - 1) * 10;
        score = Mathf.Max(baseScore - deduction, 10);
    }

    void SetResultBasedOnScore()
    {
        if (score >= 50)
            Result = MinigameManager.ResultType.Perfect;
        else if (score >= 30)
            Result = MinigameManager.ResultType.Success;
        else
            Result = MinigameManager.ResultType.Fail;
    }

    public override IEnumerator Run()
    {
        Result = MinigameManager.ResultType.Fail;
        StartNewGame();

        while (!gameOver)
            yield return null;
    }
}
