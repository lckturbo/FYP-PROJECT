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

    // Start Puzzle (Called from an external trigger)
    public void StartSequence()
    {
        currentIndex = -1;
        LoadNextStage();
    }

    private void LoadNextStage()
    {
        currentIndex++;

        // Clear the old stage
        if (currentStageInstance != null)
        {
            Destroy(currentStageInstance);
            DestroyArrow();

            currentStageInstance = null;
        }

        // When everything is finished, it's over.
        if (currentIndex >= stagePrefabs.Count)
        {
            Debug.Log("PuzzleGameManager: All stages cleared!");

            // Player movement re-enabled
            if (PlayerController.Instance != null)
                PlayerController.Instance.SetCanMove(true);

            return;
        }

        // Generate a prefab and attach it to the UI
        GameObject prefab = stagePrefabs[currentIndex];
        if (prefab == null)
        {
            Debug.LogError($"PuzzleGameManager: stagePrefabs[{currentIndex}] is null");
            return;
        }

        currentStageInstance = Instantiate(prefab, puzzleUI);

        // Search for IcePuzzle and initialize
        IcePuzzle puzzle = currentStageInstance.GetComponentInChildren<IcePuzzle>();
        if (puzzle == null)
        {
            Debug.LogError("PuzzleGameManager: IcePuzzle not found in StagePrefab.");
            return;
        }

        // Callback registration upon clearing
        puzzle.OnStageClear = () =>
        {
            Debug.Log($"Stage {currentIndex} cleared!");
            LoadNextStage();
        };

        // Start
        puzzle.OpenPuzzle();

        // Pass to the input script
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
