using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class IcePuzzle : MonoBehaviour
{
    [Header("Composition")]
    [SerializeField] private GameObject grid;           // Grid (parent of Tilemap) directly under Prefab
    [SerializeField] private Tilemap tilemap;           // Stage Tilemap
    [SerializeField] private Transform playerIcon;      // Player piece

    [Header("Tiles")]
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase blueTile;
    [SerializeField] private TileBase greenTile;
    [SerializeField] private TileBase goalTile;
    [SerializeField] private TileBase startTile;

    [Header("Stage Setup")]
    [SerializeField] private int allowedMoves = 3;      // The maximum number of inputs allowed for this stage (set in the Inspector)

    private float slideSpeed = 5.0f;    // Speed per tile (units/second)

    // Status
    private Vector3Int playerPos;    // Current cell coordinates
    private Vector3Int startPos;     // Start cell
    private Vector3Int goalPos;      // Goal Cell
    private bool isPlaying = false;   // Playing flag (slide in progress)
    private bool isAcceptingInput = false; // Input Accepting Flag
    private bool reachedGoal = false;

    // Input queue (processed sequentially)
    private Queue<Vector2Int> moveQueue = new();
    private List<Vector2Int> queuedList = new(); // For debug display

    // Save the initial tile state and restore it upon reset.
    private Dictionary<Vector3Int, TileBase> initialTiles = new Dictionary<Vector3Int, TileBase>();

    public System.Action OnStageClear; // Stage Clear Notification

    #region Unity
    void Awake()
    {
        // safety checks
        if (tilemap == null) Debug.LogError("IcePuzzle: tilemap not assigned.");
        if (playerIcon == null) Debug.LogError("IcePuzzle: playerIcon not assigned.");
    }

    void Start()
    {
        // Save: Preserve the initial state of the Tilemap (all within the range)
        BoundsInt bounds = tilemap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            TileBase t = tilemap.GetTile(pos);
            initialTiles[pos] = t;

            if (t == startTile)
            {
                startPos = pos;
            }

            if (t == goalTile)
            {
                goalPos = pos;
            }
        }

        if (startPos == null || startPos == new Vector3Int())
        {
            // If start is not found, issue a warning (check logs as it may conflict with 0,0,0)
            Debug.LogWarning("IcePuzzle: The startTile may be missing. Please check the tile placement and the Inspector.");
        }

        // Place the player at the starting position (safest to do this at the end of the Start phase)
        playerPos = startPos;
        playerIcon.position = tilemap.GetCellCenterWorld(playerPos);

    }
    #endregion

    #region Open/Close / Initial state
    // Called from outside (PuzzleGameManager or Trigger)
    public void OpenPuzzle()
    {
        // Reset and display
        RestoreInitialTiles();
        playerPos = startPos;
        playerIcon.position = tilemap.GetCellCenterWorld(playerPos);
        moveQueue.Clear();
        queuedList.Clear();
        reachedGoal = false;
        isPlaying = false;
        isAcceptingInput = true;

        Debug.Log($"IcePuzzle: Opened. allowedMoves={allowedMoves}");
    }

    public void ClosePuzzle()
    {
        isAcceptingInput = false;
        moveQueue.Clear();
        queuedList.Clear();
        reachedGoal = false;
        isPlaying = false;

    }

    // Reset to initial tile placement
    private void RestoreInitialTiles()
    {
        foreach (var kv in initialTiles)
        {
            tilemap.SetTile(kv.Key, kv.Value);
        }
        tilemap.RefreshAllTiles();
    }
    #endregion

    #region Input queue API (Call from outside)
    // Called from the inspector or a different script (the return value indicates whether the enqueue was successful)
    public bool QueueMove(Vector2Int dir)
    {
        if (!isAcceptingInput)
        {
            Debug.Log("IcePuzzle: We are not currently accepting entries.");
            return false;
        }

        if (moveQueue.Count >= allowedMoves)
        {
            Debug.Log("IcePuzzle: No further input will be accepted.");
            return false;
        }

        moveQueue.Enqueue(dir);
        queuedList.Add(dir);

        Debug.Log($"IcePuzzle: Remember input {dir} (Queue: {FormatQueued()})");

        // Playback starts automatically when the allowed number of inputs is reached
        if (moveQueue.Count >= allowedMoves)
        {
            Debug.Log("IcePuzzle: The maximum number of inputs allowed has been reached and the video will automatically play.");
            StartMove();
        }

        return true;
    }

    // Call this when you want to start playback manually (e.g., with a confirmation key)
    public void StartMove()
    {
        if (isPlaying) return;
        if (moveQueue.Count == 0) { Debug.Log("IcePuzzle: No moves to play"); return; }

        isAcceptingInput = false;
        isPlaying = true;
        StartCoroutine(PlayMovesCoroutine());
    }

    // Stringify the current input column for debugging purposes
    private string FormatQueued()
    {
        if (queuedList.Count == 0) return "[]";
        return "[" + string.Join(", ", queuedList.ConvertAll(d => d.ToString()).ToArray()) + "]";
    }
    #endregion

    #region Play / Slide coroutines
    // A coroutine that executes in order in the directions in the queue
    private IEnumerator PlayMovesCoroutine()
    {
        reachedGoal = false;

        while (moveQueue.Count > 0 && !reachedGoal)
        {
            Vector2Int dir = moveQueue.Dequeue();
            if (queuedList.Count > 0) queuedList.RemoveAt(0);

            Debug.Log($"IcePuzzle: Start execution dir={dir} (remaining {moveQueue.Count})");
            yield return StartCoroutine(SlideCoroutine(dir));
        }

        isPlaying = false;

        if (reachedGoal)
        {
            Debug.Log("IcePuzzle: Stage clear notification");
            OnStageClear?.Invoke();
        }
        else
        {
            Debug.Log("IcePuzzle: Incorrect. Reset the stage.");

                   // <<< NEW : Clear arrows when FAIL
                   if (PuzzleGameManager.Instance != null)
                PuzzleGameManager.Instance.DestroyArrow();

            ResetStageToStart();
            isAcceptingInput = true;
        }
    }


    // Slide cell by cell in one direction until hitting a wall (with animation)
    private IEnumerator SlideCoroutine(Vector2Int dir)
    {
        while (true)
        {
            Vector3Int next = playerPos + new Vector3Int(dir.x, dir.y, 0);
            TileBase nextTile = tilemap.GetTile(next);

            // If it's a wall or outside the board, the slide is over (the slide in this direction ends).
            if (nextTile == null || nextTile == wallTile)
            {
                // Debug.Log($"Slide: Next is wall or null ({next}) -> stop");
                yield break;
            }

            // Target position in world coordinates
            Vector3 from = tilemap.GetCellCenterWorld(playerPos);
            Vector3 to = tilemap.GetCellCenterWorld(next);

            float distance = Vector3.Distance(from, to);
            float duration = Mathf.Max(0.0001f, distance / slideSpeed);
            float t = 0f;

            // Move it little by little to create the appearance of sliding
            while (t < duration)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Clamp01(t / duration);
                playerIcon.position = Vector3.Lerp(from, to, alpha);
                yield return null;
            }

            // Accurately set the target position
            playerIcon.position = to;
            playerPos = next;

            // Reacquire tiles on the squares you have reached (because their state may have changed)
            TileBase curTile = tilemap.GetTile(playerPos);

            // Step on a blue tile -> Replace it with a wall
            if (curTile == blueTile)
            {
                tilemap.SetTile(playerPos, wallTile);
                Debug.Log($"IcePuzzle: Step on blue tile -> Wall at {playerPos}");
            }
            // Step on a green tile -> change it to a blue tile
            else if (curTile == greenTile)
            {
                tilemap.SetTile(playerPos, blueTile);
                Debug.Log($"IcePuzzle: Step on a green tile -> turns blue at {playerPos}");
            }
            // Reaching the goal
            else if (curTile == goalTile)
            {
                reachedGoal = true;
                Debug.Log($"IcePuzzle: Reach the goal at {playerPos}");
                yield break;
            }

            // Continue moving in the same direction (continue loop)
        }
    }
    #endregion

    #region Reset / helpers
    // Reset when incorrect (return appearance to initial state)
    private void ResetStageToStart()
    {
        RestoreInitialTiles();
        moveQueue.Clear();
        queuedList.Clear();
        playerPos = startPos;
        playerIcon.position = tilemap.GetCellCenterWorld(playerPos);
        reachedGoal = false;
        isPlaying = false;

        Debug.Log("IcePuzzle: The stage has returned to its initial state.");
    }
    #endregion

    #region External Reference Properties
    // A property for external users to check whether input is possible
    public bool IsAcceptingInput()
    {
        return isAcceptingInput && !isPlaying;
    }

    // Returns what's currently in the queue for debugging and UI display
    public string GetQueuedDebugString()
    {
        return FormatQueued();
    }

    // For configuration reference
    public int GetAllowedMoves() => allowedMoves;
    #endregion
}
