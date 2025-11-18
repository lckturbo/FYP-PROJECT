using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleGameManager : MonoBehaviour
{
    [Header("StagePrefab")]
    [SerializeField] private List<GameObject> stagePrefabs = new List<GameObject>();

    [Header("PuzzleUI Parent")]
    [SerializeField] private Transform puzzleUI;

    [Header("ArrowUI")]
    [SerializeField] private ArrowManager arrowManager;

    [Header("Camera Panning")]
    [SerializeField] private Transform panPoint1;
    [SerializeField] private Transform panPoint2;

    [SerializeField] private List<GameObject> barriersToRemove;
    [SerializeField] private List<Animator> animatorsToTrigger;

    [SerializeField] private float panDuration = 0.6f;
    [SerializeField] private float holdOnEach = 0.3f;

    private bool puzzleSolved = false;


    private int currentIndex = -1;
    private GameObject currentStageInstance;
    private PuzzleTrigger activeTrigger; // << store source trigger

    public static PuzzleGameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }


    // Start Puzzle (Called from an external trigger)
    public void StartSequenceFromTrigger(PuzzleTrigger trigger)
    {
        activeTrigger = trigger;
        currentIndex = -1;
        LoadNextStage();
    }

    private void LoadNextStage()
    {
        currentIndex++;

        if (currentStageInstance != null)
        {
            Destroy(currentStageInstance);
            DestroyArrow();
            currentStageInstance = null;
        }

        if (currentIndex >= stagePrefabs.Count)
        {
            // Finished puzzle ? start reveal sequence
            StartCoroutine(FinishPuzzleSequence());
            return;
        }


        GameObject prefab = stagePrefabs[currentIndex];

        currentStageInstance = Instantiate(prefab, puzzleUI);

        // Spawn at trigger world position
        if (activeTrigger != null)
        {
            Transform stageRoot = currentStageInstance.transform.Find("StageRoot");
            if (stageRoot != null)
                stageRoot.position = activeTrigger.SpawnPosition;
            else
                currentStageInstance.transform.position = activeTrigger.SpawnPosition;
        }

        IcePuzzle puzzle = currentStageInstance.GetComponentInChildren<IcePuzzle>();
        if (puzzle == null)
        {
            Debug.LogError("PuzzleGameManager: IcePuzzle not found in StagePrefab.");
            return;
        }

        puzzle.OnStageClear = () =>
        {
            LoadNextStage();
        };

        puzzle.OpenPuzzle();

        IcePuzzleInput input = GetComponent<IcePuzzleInput>();
        if (input != null)
        {
            input.enabled = true;
            input.GetType().GetField("puzzle",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .SetValue(input, puzzle);
        }
    }
    private IEnumerator FinishPuzzleSequence()
    {
        if (puzzleSolved) yield break;   // prevent multiple runs
        puzzleSolved = true;

        // Disable movement
        NewPlayerMovement playerMovement = FindObjectOfType<NewPlayerMovement>();
        if (playerMovement != null) playerMovement.enabled = false;

        PlayerInput playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null) playerInput.enabled = false;

        // Camera
        NewCameraController cam = FindObjectOfType<NewCameraController>();

        // Player anchor (to return camera)
        Vector3 playerPos = FindObjectOfType<NewPlayerMovement>().transform.position;
        Transform playerAnchor = CreateTempAnchor(playerPos, "PuzzleCam_Player");

        // === PAN POINT 1 ===
        if (cam && panPoint1)
        {
            yield return cam.PanTo(panPoint1, panDuration);
            yield return new WaitForSeconds(holdOnEach);
        }

        // === PAN POINT 2 ===
        if (cam && panPoint2)
        {
            yield return cam.PanTo(panPoint2, panDuration);
            yield return new WaitForSeconds(holdOnEach);
        }

        // === Remove barriers ===
        foreach (var b in barriersToRemove)
            if (b != null) Destroy(b);

        // === Trigger animations ===
        foreach (var anim in animatorsToTrigger)
            if (anim != null) anim.SetTrigger("Solved");

        // === Return camera ===
        if (cam && playerAnchor)
            yield return cam.PanTo(playerAnchor, panDuration);

        if (cam)
            yield return cam.ReturnToPlayer(panDuration);

        Destroy(playerAnchor.gameObject);

        // Re-enable movement
        if (playerInput != null) playerInput.enabled = true;
        if (playerMovement != null) playerMovement.enabled = true;

        // Remove the trigger so the puzzle cannot restart again
        if (activeTrigger != null)
            Destroy(activeTrigger.gameObject);

        Debug.Log("Puzzle Completed.");
    }

    private Transform CreateTempAnchor(Vector3 pos, string name)
    {
        GameObject temp = new GameObject(name);
        temp.transform.position = pos;
        return temp.transform;
    }


    public void DestroyArrow()
    {
        arrowManager.ClearArrows();
    }
}
