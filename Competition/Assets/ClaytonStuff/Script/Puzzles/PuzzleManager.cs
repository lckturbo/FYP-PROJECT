using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class PuzzleGroup
{
    [Tooltip("Unique ID for saving/loading this puzzle group")]
    public string puzzleID;

    [Tooltip("Goals that must all be occupied to solve this group.")]
    public List<PuzzleGoal> goals = new List<PuzzleGoal>();

    [Tooltip("Barriers or gates that will be removed when this group is solved.")]
    public List<GameObject> barriers = new List<GameObject>();

    [Tooltip("Optional animators to trigger when solved")]
    public List<Animator> animators = new List<Animator>();

    [Header("Camera Pan Points")]
    public Transform panPoint1;   // first focus point
    public Transform panPoint2;   // second focus point

    [HideInInspector] public bool solved = false;
}

public class PuzzleManager : MonoBehaviour, IDataPersistence
{
    [Header("Puzzle Groups")]
    public List<PuzzleGroup> puzzleGroups = new List<PuzzleGroup>();

    [Header("Camera Timing")]
    public float panDuration = 0.6f;
    public float holdOnEach = 0.3f;

    public event System.Action<PuzzleGroup> OnAnyPuzzleGroupSolved;

    private void Start()
    {
        foreach (var group in puzzleGroups)
        {
            foreach (var goal in group.goals)
            {
                if (goal != null)
                {
                    goal.OnOccupied += (g, block) => CheckGroupSolved(group);
                    goal.OnEmptied += (g) => CheckGroupSolved(group);
                }
            }
            CheckGroupSolved(group);
        }
    }

    private void CheckGroupSolved(PuzzleGroup group)
    {
        if (group.solved) return;

        foreach (var goal in group.goals)
        {
            if (goal == null || !goal.IsOccupied)
                return;
        }

        group.solved = true;
        StartCoroutine(HandlePuzzleSolved(group));
    }

    private IEnumerator HandlePuzzleSolved(PuzzleGroup group)
    {
        Debug.Log($"Puzzle group solved: {group.puzzleID}");

        // --- Disable player control ---
        PlayerInput playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null) playerInput.enabled = false;

        NewPlayerMovement newPlayerMovement = FindAnyObjectByType<NewPlayerMovement>();
        if (newPlayerMovement != null) newPlayerMovement.enabled = false;

        NewCameraController cam = FindObjectOfType<NewCameraController>();
        Transform playerAnchor = CreateTempAnchor(FindPlayerPosition(), "CamTemp_Player");

        // === PAN TO POINT 1 ===
        if (cam && group.panPoint1)
        {
            yield return cam.PanTo(group.panPoint1, panDuration);
            yield return new WaitForSeconds(holdOnEach);
        }

        // === PAN TO POINT 2 ===
        if (cam && group.panPoint2)
        {
            yield return cam.PanTo(group.panPoint2, panDuration);
            yield return new WaitForSeconds(holdOnEach);
        }

        // === Trigger Animations & Barrier Removal ===
        foreach (var barrier in group.barriers)
        {
            if (barrier != null)
                Destroy(barrier);
        }

        foreach (var anim in group.animators)
        {
            if (anim != null)
                anim.SetTrigger("Solved");
        }

        // === Return to Player ===
        if (cam && playerAnchor)
            yield return cam.PanTo(playerAnchor, panDuration);

        if (cam)
            yield return cam.ReturnToPlayer(panDuration);

        Destroy(playerAnchor.gameObject);

        // --- Re-enable player control ---
        if (playerInput != null) playerInput.enabled = true;

        if (newPlayerMovement != null) newPlayerMovement.enabled = true;

        OnAnyPuzzleGroupSolved?.Invoke(group);
    }


    private Vector3 FindPlayerPosition()
    {
        var player = FindObjectOfType<NewPlayerMovement>();
        if (player != null)
            return player.transform.position;
        return Vector3.zero;
    }

    private Transform CreateTempAnchor(Vector3 pos, string name)
    {
        GameObject temp = new GameObject(name);
        temp.transform.position = pos;
        return temp.transform;
    }

    // ===== SAVE / LOAD =====
    public void SaveData(ref GameData data)
    {
        foreach (var group in puzzleGroups)
        {
            var existing = data.savedPuzzles.Find(p => p.puzzleID == group.puzzleID);
            if (existing != null)
                existing.solved = group.solved;
            else
                data.savedPuzzles.Add(new GameData.PuzzleSaveEntry { puzzleID = group.puzzleID, solved = group.solved });
        }
    }

    public void LoadData(GameData data)
    {
        foreach (var entry in data.savedPuzzles)
        {
            var group = puzzleGroups.Find(g => g.puzzleID == entry.puzzleID);
            if (group == null) continue;

            group.solved = entry.solved;

            if (group.solved)
            {
                foreach (var barrier in group.barriers)
                    if (barrier != null) Destroy(barrier);

                foreach (var anim in group.animators)
                    if (anim != null) anim.Play("Solved", 0, 1f);
            }
        }
    }
}
