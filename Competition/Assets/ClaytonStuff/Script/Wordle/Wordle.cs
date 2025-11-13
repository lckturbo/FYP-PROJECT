    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Networking;

public class Wordle : BaseMinigame
{
    [Header("UI References")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private GameObject letterBoxPrefab;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text topicText;

    [System.Serializable]
    public class WordTopic
    {
        public string topicName;
        public string[] words;
    }

    [Header("Game Settings")]
    [SerializeField] private int maxAttempts = 6;
    [SerializeField] private float totalGameTime = 30f; //  total time for entire game

    [Header("Word Topics")]
    [SerializeField] private WordTopic[] topics;

    [Header("Animator")]
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject animationPanel;
    [SerializeField] private GameObject tutoralPanel;
    //[SerializeField] private GameObject minigamePanel;

    private string targetWord;
    private int currentAttempt = 0;
    private List<List<TMP_Text>> grid = new List<List<TMP_Text>>();
    private string currentGuess = "";
    private float timer;
    private bool gameOver = false;
    private int score = 0;

    private bool canStartTimer = false;

    public void Start()
    {
        SetupGrid();
        //StartNewGame();
    }

    public void BeginMinigame()
    {
        StartNewGame();    // sets up topic, words, etc.
        canStartTimer = true;
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
        if (topics == null || topics.Length == 0)
        {
            Debug.LogError("No word topics assigned!");
            return;
        }

        WordTopic selectedTopic = topics[Random.Range(0, topics.Length)];
        string[] list = selectedTopic.words;
        if (list == null || list.Length == 0)
        {
            Debug.LogError($"Topic '{selectedTopic.topicName}' has no words!");
            return;
        }

        targetWord = list[Random.Range(0, list.Length)].ToUpper();
        currentAttempt = 0;
        score = 0;
        gameOver = false;
        currentGuess = "";

        timer = totalGameTime; //  total timer starts here

        if (topicText)
            topicText.text = $"Topic: <b>{selectedTopic.topicName}</b>";

        resultText.text = "Type a 5-letter word!";
        UpdateLetterRow();
        UpdateTimerUI();

        Debug.Log($"[Wordle] Topic: {selectedTopic.topicName}, Target: {targetWord}");
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
        if (!canStartTimer || gameOver) return;

        timer -= Time.unscaledDeltaTime;
        UpdateTimerUI();

        if (timer <= 0f)
        {
            timer = 0;
            gameOver = true;

            //  Show the correct word when time runs out
            resultText.text = $"Time’s up! The word was <b>{targetWord}</b>.";

            StartCoroutine(EndGameWithDelay());
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
        timerText.text = $"{timer:F1}s";
    }

    IEnumerator CheckWordValidity(string guess)
    {
        resultText.text = $"Checking if '{guess}' is a real word...";

        string url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{guess.ToLower()}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            string json = request.downloadHandler.text;

            if (json.Contains("\"word\""))
            {
                OnSubmitGuess(false);
            }
            else
            {
                resultText.text = $"'{guess}' is not a valid English word!";
                currentGuess = "";

                if (currentAttempt < grid.Count)
                {
                    var row = grid[currentAttempt];
                    foreach (var letter in row)
                    {
                        letter.text = "";
                        letter.color = Color.white;
                    }
                }
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
            resultText.text = $"Correct! The word was {targetWord}\nScore: {score}";
            StartCoroutine(EndGameWithDelay());
            return;
        }

        currentAttempt++;
        currentGuess = "";

        //  No timer reset here anymore

        if (currentAttempt >= maxAttempts)
        {
            score = 0;
            resultText.text = $"Out of attempts!\nWord was: {targetWord}\nScore: {score}";
            StartCoroutine(EndGameWithDelay());
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
        int timeBonus = Mathf.RoundToInt(timer); //  optional time bonus
        score = Mathf.Max(baseScore - deduction + timeBonus, 10);
    }

    IEnumerator EndGameWithDelay()
    {
        gameOver = true;
        yield return new WaitForSecondsRealtime(2f);
        EndGame();
    }

    void EndGame()
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
        BattleManager.instance?.SetBattlePaused(true);

        animationPanel.SetActive(true);
        tutoralPanel.SetActive(false);
        //minigamePanel.SetActive(false);

        if (anim)
        {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            anim.SetTrigger("start");
            yield return new WaitForSecondsRealtime(1.5f);
        }

        animationPanel.SetActive(false);
        tutoralPanel.SetActive(true);
        //minigamePanel.SetActive(true);

        while (!gameOver)
            yield return null;

        yield return new WaitForSecondsRealtime(2f);

        BattleManager.instance?.SetBattlePaused(false);
    }
}
