using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class ScrabbleGame : MonoBehaviour
{
    [Header("UI References")]
    public Transform boardParent;          // 15x15 board parent
    public GameObject boardTilePrefab;     // Prefab for board tile
    public Transform playerRackParent;     // Parent for player letters
    public GameObject letterTilePrefab;    // Prefab for letter tile
    public TMP_Text messageText;           // Status messages
    public TMP_Text scoreText;             // Score display
    public Button submitButton;            // Button to submit words
    public Button resetTurnButton;         // Button to undo placed tiles

    [Header("Game Settings")]
    public int boardSize = 15;
    public int rackSize = 7;
    public string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private Button[,] boardTiles;
    private List<char> playerRack = new List<char>();
    private List<GameObject> rackTileObjects = new List<GameObject>();
    private Dictionary<char, int> letterScores = new Dictionary<char, int>();

    private List<(int x, int y, char c)> currentTurnTiles = new List<(int, int, char)>();
    private GameObject selectedRackTile = null;
    private int totalScore = 0;

    // Simple dictionary for demo (you can replace with a real one)
    private HashSet<string> dictionary = new HashSet<string>()
    {
        "HELLO", "WORLD", "UNITY", "GAME", "CODE", "SCRABBLE", "PLAY", "FUN", "WORD","TO"
    };

    void Start()
    {
        SetupLetterScores();
        SetupBoard();
        SetupRack();

        submitButton.onClick.AddListener(OnSubmitWord);
        resetTurnButton.onClick.AddListener(OnResetTurn);
        UpdateScoreUI();
    }

    void SetupLetterScores()
    {
        // English Scrabble letter values
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
                text.text = ""; // Empty initially

                int bx = x, by = y;
                tile.GetComponent<Button>().onClick.AddListener(() => OnBoardTileClicked(bx, by));

                boardTiles[x, y] = tile.GetComponent<Button>();
            }
        }

        // Add some random pre-filled tiles
        AddStartingLetters();
    }

    void AddStartingLetters()
    {
        // Example: randomly place 5–8 letters on the board
        int numberOfLetters = Random.Range(5, 9);

        for (int i = 0; i < numberOfLetters; i++)
        {
            int x = Random.Range(0, boardSize);
            int y = Random.Range(0, boardSize);

            TMP_Text text = boardTiles[x, y].GetComponentInChildren<TMP_Text>();
            if (string.IsNullOrEmpty(text.text))
            {
                char randomLetter = letters[Random.Range(0, letters.Length)];
                text.text = randomLetter.ToString();

                // Optional: disable clicking on pre-filled letters
                boardTiles[x, y].interactable = false;
            }
        }
    }


    void SetupRack()
    {
        // Clear old rack
        foreach (var obj in rackTileObjects) Destroy(obj);
        rackTileObjects.Clear();
        playerRack.Clear();

        for (int i = 0; i < rackSize; i++)
        {
            AddRandomLetterToRack();
        }
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
            messageText.text = "Select a letter from your rack first!";
            return;
        }

        TMP_Text tileText = boardTiles[x, y].GetComponentInChildren<TMP_Text>();
        if (!string.IsNullOrEmpty(tileText.text))
        {
            messageText.text = "That board tile is already occupied!";
            return;
        }

        // Place selected letter
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

        StartCoroutine(CheckWordOnline(formedWord.ToUpper()));
    }

    IEnumerator CheckWordOnline(string word)
    {
        string url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";
        messageText.text = $"?? Checking '{word}'...";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                messageText.text = $"Network error: {request.error}";
                OnResetTurn();
                yield break;
            }

            string jsonResponse = request.downloadHandler.text;

            if (jsonResponse.Contains("\"word\"")) // API returns valid word JSON
            {
                int wordScore = CalculateWordScore(word);
                totalScore += wordScore;
                messageText.text = $"? '{word}' is valid! You earned {wordScore} points.";
                currentTurnTiles.Clear();
                UpdateScoreUI();
                RefillRack();
            }
            else
            {
                messageText.text = $"? '{word}' not found in dictionary!";
                OnResetTurn();
            }
        }
    }


    public void OnResetHand()
    {
        // Clear current rack
        foreach (var obj in rackTileObjects)
        {
            Destroy(obj);
        }
        rackTileObjects.Clear();
        playerRack.Clear();

        // Fill with new random letters
        for (int i = 0; i < rackSize; i++)
        {
            AddRandomLetterToRack();
        }

        messageText.text = "?? Hand reset! You got new letters.";
    }


    string GetFormedWord()
    {
        if (currentTurnTiles.Count == 0)
            return null;

        // Check if all tiles are in same row or same column
        bool sameRow = true, sameCol = true;
        int firstX = currentTurnTiles[0].x;
        int firstY = currentTurnTiles[0].y;

        foreach (var tile in currentTurnTiles)
        {
            if (tile.x != firstX) sameCol = false;
            if (tile.y != firstY) sameRow = false;
        }

        if (!sameRow && !sameCol)
            return null;

        // Sort placed tiles
        currentTurnTiles.Sort((a, b) => sameRow ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));

        string word = "";
        int startX = currentTurnTiles[0].x;
        int startY = currentTurnTiles[0].y;

        // If horizontal
        if (sameRow)
        {
            // Go left to find start
            int x = startX;
            while (x > 0)
            {
                string left = boardTiles[x - 1, startY].GetComponentInChildren<TMP_Text>().text;
                if (string.IsNullOrEmpty(left)) break;
                x--;
            }

            // Build word to the right
            for (; x < boardSize; x++)
            {
                string letter = boardTiles[x, startY].GetComponentInChildren<TMP_Text>().text;
                if (string.IsNullOrEmpty(letter)) break;
                word += letter;
            }
        }
        else // Vertical
        {
            // Go up to find start
            int y = startY;
            while (y > 0)
            {
                string up = boardTiles[firstX, y - 1].GetComponentInChildren<TMP_Text>().text;
                if (string.IsNullOrEmpty(up)) break;
                y--;
            }

            // Build word downward
            for (; y < boardSize; y++)
            {
                string letter = boardTiles[firstX, y].GetComponentInChildren<TMP_Text>().text;
                if (string.IsNullOrEmpty(letter)) break;
                word += letter;
            }
        }

        return word;
    }


    int CalculateWordScore(string word)
    {
        int score = 0;
        foreach (char c in word)
        {
            score += letterScores.ContainsKey(c) ? letterScores[c] : 1;
        }
        return score;
    }

    void RefillRack()
    {
        while (playerRack.Count < rackSize)
        {
            AddRandomLetterToRack();
        }
    }

    void OnResetTurn()
    {
        foreach (var t in currentTurnTiles)
        {
            TMP_Text text = boardTiles[t.x, t.y].GetComponentInChildren<TMP_Text>();
            text.text = "";
        }

        // Return tiles to rack
        foreach (var t in currentTurnTiles)
        {
            GameObject tile = Instantiate(letterTilePrefab, playerRackParent);
            TMP_Text text = tile.GetComponentInChildren<TMP_Text>();
            text.text = t.c.ToString();
            tile.GetComponent<Button>().onClick.AddListener(() => OnRackTileClicked(tile));
            rackTileObjects.Add(tile);
        }

        currentTurnTiles.Clear();
        messageText.text = "Turn reset!";
    }

    void UpdateScoreUI()
    {
        scoreText.text = $"Score: {totalScore}";
    }
}
