using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class ScrabbleGame : BaseMinigame
{
    [Header("UI References")]
    public Transform boardParent;
    public GameObject boardTilePrefab;
    public Transform playerRackParent;
    public GameObject letterTilePrefab;
    public TMP_Text messageText;
    public TMP_Text scoreText;
    public TMP_Text timerText;         // ?? Added for countdown
    public Button submitButton;
    public Button resetTurnButton;

    [Header("Game Settings")]
    public int boardSize = 15;
    public int rackSize = 7;
    public float totalTime = 60f;      // ?? 60-second timer
    public string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private Button[,] boardTiles;
    private List<char> playerRack = new List<char>();
    private List<GameObject> rackTileObjects = new List<GameObject>();
    private Dictionary<char, int> letterScores = new Dictionary<char, int>();
    private List<(int x, int y, char c)> currentTurnTiles = new List<(int, int, char)>();
    private GameObject selectedRackTile = null;
    private int totalScore = 0;

    private float timer;
    private bool gameOver = false;

    private HashSet<string> dictionary = new HashSet<string>()
    {
        "HELLO", "WORLD", "UNITY", "GAME", "CODE", "SCRABBLE", "PLAY", "FUN", "WORD", "TO"
    };

    void Start()
    {
        SetupLetterScores();
        SetupBoard();
        SetupRack();

        submitButton.onClick.AddListener(OnSubmitWord);
        resetTurnButton.onClick.AddListener(OnResetTurn);
        UpdateScoreUI();

        StartTimer();
    }

    void StartTimer()
    {
        timer = totalTime;
        gameOver = false;
        StartCoroutine(TimerRoutine());
    }

    IEnumerator TimerRoutine()
    {
        while (timer > 0 && !gameOver)
        {
            timer -= Time.deltaTime;
            UpdateTimerUI();
            yield return null;
        }

        if (!gameOver)
        {
            messageText.text = "? Time’s up!";
            EndGame();
        }
    }

    void UpdateTimerUI()
    {
        timerText.text = $"Time: {timer:F1}s";
    }

    void SetupLetterScores()
    {
        string one = "AEILNORSTU";
        string two = "DG";
        string three = "BCMP";
        string four = "FHVWY";
        string five = "K";
        string eight = "JX";
        string ten = "QZ";

        foreach (char c in one) letterScores[c] = 1;
        foreach (char c in two) letterScores[c] = 2;
        foreach (char c in three) letterScores[c] = 3;
        foreach (char c in four) letterScores[c] = 4;
        foreach (char c in five) letterScores[c] = 5;
        foreach (char c in eight) letterScores[c] = 8;
        foreach (char c in ten) letterScores[c] = 10;
    }

    void SetupBoard()
    {
        boardTiles = new Button[boardSize, boardSize];
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                GameObject tile = Instantiate(boardTilePrefab, boardParent);
                tile.name = $"Tile_{x}_{y}";
                TMP_Text text = tile.GetComponentInChildren<TMP_Text>();
                text.text = "";

                int bx = x, by = y;
                tile.GetComponent<Button>().onClick.AddListener(() => OnBoardTileClicked(bx, by));
                boardTiles[x, y] = tile.GetComponent<Button>();
            }
        }

        AddStartingLetters();
    }

    void AddStartingLetters()
    {
        int numberOfLetters = Random.Range(4, 8);
        for (int i = 0; i < numberOfLetters; i++)
        {
            int x = Random.Range(0, boardSize);
            int y = Random.Range(0, boardSize);
            TMP_Text text = boardTiles[x, y].GetComponentInChildren<TMP_Text>();
            if (string.IsNullOrEmpty(text.text))
            {
                char randomLetter = letters[Random.Range(0, letters.Length)];
                text.text = randomLetter.ToString();
                boardTiles[x, y].interactable = false;
            }
        }
        messageText.text = "Random letters added to the board!";
    }

    void SetupRack()
    {
        foreach (var obj in rackTileObjects) Destroy(obj);
        rackTileObjects.Clear();
        playerRack.Clear();

        for (int i = 0; i < rackSize; i++)
            AddRandomLetterToRack();
    }

    void AddRandomLetterToRack()
    {
        char letter = letters[Random.Range(0, letters.Length)];
        playerRack.Add(letter);

        GameObject tile = Instantiate(letterTilePrefab, playerRackParent);
        TMP_Text text = tile.GetComponentInChildren<TMP_Text>();
        text.text = letter.ToString();

        Button button = tile.GetComponent<Button>();
        button.onClick.AddListener(() => OnRackTileClicked(tile));
        rackTileObjects.Add(tile);
    }

    void OnRackTileClicked(GameObject tile)
    {
        selectedRackTile = tile;
        messageText.text = $"Selected letter: {tile.GetComponentInChildren<TMP_Text>().text}";
    }

    void OnBoardTileClicked(int x, int y)
    {
        if (selectedRackTile == null)
        {
            messageText.text = "Select a letter first!";
            return;
        }

        TMP_Text tileText = boardTiles[x, y].GetComponentInChildren<TMP_Text>();
        if (!string.IsNullOrEmpty(tileText.text))
        {
            messageText.text = "That spot is occupied!";
            return;
        }

        char letter = selectedRackTile.GetComponentInChildren<TMP_Text>().text[0];
        tileText.text = letter.ToString();
        currentTurnTiles.Add((x, y, letter));

        rackTileObjects.Remove(selectedRackTile);
        Destroy(selectedRackTile);
        selectedRackTile = null;
    }

    void OnSubmitWord()
    {
        if (currentTurnTiles.Count == 0)
        {
            messageText.text = "You must place at least one tile!";
            return;
        }

        string formedWord = GetFormedWord();
        if (string.IsNullOrEmpty(formedWord))
        {
            messageText.text = "Tiles must form a straight word!";
            OnResetTurn();
            return;
        }

        if (formedWord.Length <= 1)
        {
            messageText.text = $"'{formedWord}' is too short!";
            OnResetTurn();
            return;
        }

        StartCoroutine(CheckWordOnline(formedWord.ToUpper()));
    }

    IEnumerator CheckWordOnline(string word)
    {
        string url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";
        messageText.text = $"Checking '{word}'...";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
        }

        int wordScore = CalculateWordScore(word);
        totalScore += wordScore;
        messageText.text = $"'{word}' is valid! +{wordScore} points.";
        UpdateScoreUI();

        EndGame();
    }

    string GetFormedWord()
    {
        if (currentTurnTiles.Count == 0) return null;

        bool sameRow = true, sameCol = true;
        int fx = currentTurnTiles[0].x, fy = currentTurnTiles[0].y;

        foreach (var t in currentTurnTiles)
        {
            if (t.x != fx) sameCol = false;
            if (t.y != fy) sameRow = false;
        }

        if (!sameRow && !sameCol) return null;

        currentTurnTiles.Sort((a, b) => sameRow ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));

        string word = "";
        if (sameRow)
        {
            int y = fy;
            for (int x = 0; x < boardSize; x++)
            {
                string letter = boardTiles[x, y].GetComponentInChildren<TMP_Text>().text;
                if (!string.IsNullOrEmpty(letter)) word += letter;
            }
        }
        else
        {
            int x = fx;
            for (int y = 0; y < boardSize; y++)
            {
                string letter = boardTiles[x, y].GetComponentInChildren<TMP_Text>().text;
                if (!string.IsNullOrEmpty(letter)) word += letter;
            }
        }
        return word;
    }

    int CalculateWordScore(string word)
    {
        int score = 0;
        foreach (char c in word)
            score += letterScores.ContainsKey(c) ? letterScores[c] : 1;
        return score;
    }

    void OnResetTurn()
    {
        foreach (var t in currentTurnTiles)
        {
            TMP_Text text = boardTiles[t.x, t.y].GetComponentInChildren<TMP_Text>();
            text.text = "";
        }
        currentTurnTiles.Clear();
    }

    void UpdateScoreUI()
    {
        scoreText.text = $"Score: {totalScore}";
    }

    void EndGame()
    {
        gameOver = true;
        StopAllCoroutines();

        if (totalScore >= 30)
            Result = MinigameManager.ResultType.Perfect;
        else if (totalScore >= 10)
            Result = MinigameManager.ResultType.Success;
        else
            Result = MinigameManager.ResultType.Fail;

        messageText.text += $"\nGame Over! Final Score: {totalScore}";
    }

    public override IEnumerator Run()
    {
        Result = MinigameManager.ResultType.Fail;
        StartTimer();

        while (!gameOver)
            yield return null;
    }
}
