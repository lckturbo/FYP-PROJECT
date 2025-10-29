using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

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
    [SerializeField] private float attemptTime = 3f;

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
        foreach (char c in Input.inputString)
        {
            if (char.IsLetter(c))
            {
                if (currentGuess.Length < 5)
                {
                    currentGuess += char.ToUpper(c);
                    UpdateLetterRow();
                }
            }
            else if (c == '\b')
            {
                if (currentGuess.Length > 0)
                {
                    currentGuess = currentGuess.Substring(0, currentGuess.Length - 1);
                    UpdateLetterRow();
                }
            }
            else if (c == '\n' || c == '\r')
            {
                if (currentGuess.Length == 5)
                {
                    StartCoroutine(CheckWordValidity(currentGuess));
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
        timer -= Time.unscaledDeltaTime;
        UpdateTimerUI();

        if (timer <= 0f)
        {
            resultText.text = "? Time’s up! Moving to next attempt.";
            EndGame();
            OnSubmitGuess(forceFail: true);
        }
    }

    void UpdateLetterRow()
    {
        if (currentAttempt >= grid.Count) return;

        var row = grid[currentAttempt];
        for (int i = 0; i < row.Count; i++)
        {
            row[i].text = i < currentGuess.Length ? currentGuess[i].ToString() : "";
            row[i].color = Color.black;
        }
    }

    void UpdateTimerUI()
    {
        timerText.text = $"? {timer:F1}s";
    }

    IEnumerator CheckWordValidity(string guess)
    {
        resultText.text = $"Checking if '{guess}' is a real word...";

        string url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{guess.ToLower()}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                resultText.text = $"?? Network error checking '{guess}'";
                yield break;
            }

            string json = request.downloadHandler.text;
            if (json.Contains("\"word\"")) // means valid word found
            {
                OnSubmitGuess(false);
            }
            else
            {
                resultText.text = $"? '{guess}' is not a valid English word!";
                currentGuess = "";
                UpdateLetterRow();
            }
        }
    }

    void OnSubmitGuess(bool forceFail = false)
    {
        if (gameOver) return;

        string guess = currentGuess.ToUpper().Trim();

        if (!forceFail)
            DisplayGuess(guess);
        else
            DisplayGuess("     ");

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
                letterText.color = Color.green;
            else if (targetWord.Contains(guess[i].ToString()))
                letterText.color = Color.yellow;
            else
                letterText.color = Color.gray;
        }
    }

    void CalculateScore()
    {
        int baseScore = 60;
        int deduction = currentAttempt * 10;
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
        BattleManager.instance?.SetBattlePaused(true);
;
        while (!gameOver)
            yield return null;

        BattleManager.instance?.SetBattlePaused(false);
    }
}
