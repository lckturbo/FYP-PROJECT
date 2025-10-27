using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Wordle : BaseMinigame
{
    [Header("UI References")]
    [SerializeField] private Transform gridParent; // Parent with 6x5 letter boxes
    [SerializeField] private GameObject letterBoxPrefab;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text timerText;

    [Header("Game Settings")]
    [SerializeField] private int maxAttempts = 6;
    [SerializeField] private string[] wordList = { "APPLE", "CHAIR", "UNITY", "PLANT", "GAMES", "CLOUD" };
    [SerializeField] private float attemptTime = 10f;

    private string targetWord;
    private int currentAttempt = 0;
    private List<List<TMP_Text>> grid = new List<List<TMP_Text>>();

    private string currentGuess = "";
    private float timer;
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

        // Create 6 rows × 5 columns of letter boxes
        for (int i = 0; i < maxAttempts; i++)
        {
            List<TMP_Text> row = new List<TMP_Text>();
            for (int j = 0; j < 5; j++)
            {
                GameObject box = Instantiate(letterBoxPrefab, gridParent);
                TMP_Text letter = box.GetComponentInChildren<TMP_Text>();
                letter.text = "";
                letter.color = Color.white;
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
        currentGuess = "";
        timer = attemptTime;
        resultText.text = "Type a 5-letter word and press Enter!";
        UpdateLetterRow();
        UpdateTimerUI();

        Debug.Log($"[Wordle] Target word: {targetWord}");
    }

    void Update()
    {
        if (gameOver) return;

        HandleTyping();
        HandleTimer();
    }

    void HandleTyping()
    {
        // Loop through all characters typed this frame
        foreach (char c in Input.inputString)
        {
            if (char.IsLetter(c))
            {
                if (currentGuess.Length < 5)
                {
                    currentGuess += char.ToUpper(c);
                    UpdateLetterRow(); // instantly show letter
                }
            }
            else if (c == '\b') // Backspace
            {
                if (currentGuess.Length > 0)
                {
                    currentGuess = currentGuess.Substring(0, currentGuess.Length - 1);
                    UpdateLetterRow(); // instantly update after backspace
                }
            }
            else if (c == '\n' || c == '\r') // Enter key
            {
                if (currentGuess.Length == 5)
                {
                    OnSubmitGuess();
                }
                else
                {
                    resultText.text = "Word must be 5 letters!";
                }
            }
        }
    }

    void HandleTimer()
    {
        timer -= Time.deltaTime;
        UpdateTimerUI();

        if (timer <= 0f)
        {
            resultText.text = "? Time’s up! Moving to next attempt.";
            OnSubmitGuess(forceFail: true);
        }
    }

    void UpdateLetterRow()
    {
        if (currentAttempt >= grid.Count) return;

        var row = grid[currentAttempt];

        for (int i = 0; i < row.Count; i++)
        {
            if (i < currentGuess.Length)
            {
                row[i].text = currentGuess[i].ToString(); // show typed letter
            }
            else
            {
                row[i].text = ""; // clear unused boxes
            }

            // ?? Change color to black while typing
            row[i].color = Color.black;
        }

        Canvas.ForceUpdateCanvases();
    }


    void UpdateTimerUI()
    {
        timerText.text = $"? {timer:F1}s";
    }

    void OnSubmitGuess(bool forceFail = false)
    {
        if (gameOver) return;

        string guess = currentGuess.ToUpper().Trim();

        if (!forceFail)
            DisplayGuess(guess);
        else
            DisplayGuess("     "); // blank for timeout

        if (guess == targetWord && !forceFail)
        {
            CalculateScore();
            resultText.text = $"? Correct! The word was {targetWord}\nScore: {score}";
            EndGame();
            return;
        }

        currentAttempt++;
        currentGuess = "";
        timer = attemptTime;

        if (currentAttempt >= maxAttempts)
        {
            score = 0;
            resultText.text = $"? Out of attempts!\nWord was: {targetWord}\nScore: {score}";
            EndGame();
        }
        else
        {
            resultText.text = $"Attempt {currentAttempt + 1}/{maxAttempts}";
            UpdateLetterRow();
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
                letterText.color = Color.green; // Correct spot
            else if (targetWord.Contains(guess[i].ToString()))
                letterText.color = Color.yellow; // Wrong spot
            else
                letterText.color = Color.gray; // Not in word
        }
    }

    void CalculateScore()
    {
        int baseScore = 60;
        int deduction = (currentAttempt) * 10;
        score = Mathf.Max(baseScore - deduction, 10);
    }

    void EndGame()
    {
        gameOver = true;

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
