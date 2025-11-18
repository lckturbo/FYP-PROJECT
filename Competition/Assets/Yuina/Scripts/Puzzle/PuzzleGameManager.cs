using System.Collections.Generic;
using UnityEngine;

public class PuzzleGameManager : MonoBehaviour
{
    [Header("StagePrefab")]
    [SerializeField] private List<GameObject> stagePrefabs = new List<GameObject>();

    [Header("PuzzleUI Parent")]
    [SerializeField] private Transform puzzleUI;

    [Header("ArrowUI")]
    [SerializeField] private ArrowManager arrowManager;

    private int currentIndex = -1;
    private GameObject currentStageInstance;
    private PuzzleTrigger activeTrigger; // << store source trigger


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
            NewPlayerMovement playerMovement = FindObjectOfType<NewPlayerMovement>();
            if (playerMovement != null) playerMovement.enabled = true;
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

    public void DestroyArrow()
    {
        arrowManager.ClearArrows();
    }
}
