using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static GameData;

public class PathPuzzle : MonoBehaviour, IDataPersistence
{
    [SerializeField] private List<int> correctPath;
    private HashSet<int> solvedZoneIDs = new HashSet<int>();
    [SerializeField] private Transform cameraFocusPoint;
    [SerializeField] private float cameraPanDuration = 0.5f;
    [SerializeField] private float cameraHoldDuration = 1.0f;

    private int currentStep = 0;
    private bool canStep = false;
    private bool completed = false;
    private bool isLoadingFromSave = false;

    private List<PuzzleZone> zones;
    private NewCameraController cam;

    private bool puzzleStarted = false;
    private PuzzleZone lastZoneStepped;
    private bool sequencePlayed = false;

    private void Start()
    {
        zones = new List<PuzzleZone>(FindObjectsOfType<PuzzleZone>());
        cam = FindObjectOfType<NewCameraController>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !puzzleStarted)
        {
            PlayerInput playerInput = collision.GetComponent<PlayerInput>();
            if (playerInput != null)
                playerInput.enabled = false;
            puzzleStarted = true;
            StartCoroutine(StartPuzzleSequence());
        }
    }

    private IEnumerator StartPuzzleSequence()
    {
        if (!sequencePlayed)
        {
            sequencePlayed = true;
            yield return new WaitForSeconds(0.5f);
            if (cam && cameraFocusPoint)
            {
                yield return cam.StartCoroutine(cam.PanTo(cameraFocusPoint, cameraPanDuration));
                yield return cam.StartCoroutine(cam.ZoomTo(10f, 1f));
            }


            yield return StartCoroutine(ShowCorrectPath());
            yield return new WaitForSeconds(cameraHoldDuration);

            if (cam)
            {
                yield return cam.StartCoroutine(cam.ZoomTo(5f, 1f));
                yield return cam.StartCoroutine(cam.ReturnToPlayer(cameraPanDuration));
            }
        }

        canStep = true;
        Debug.Log("Player may now step.");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            PlayerInput playerInput = player.GetComponent<PlayerInput>();
            if (playerInput != null)
                playerInput.enabled = true;
        }
    }

    public void OnZoneStepped(PuzzleZone zone)
    {
        if (!canStep) return;
        if (lastZoneStepped == zone) return;
        lastZoneStepped = zone;

        if (solvedZoneIDs.Contains(zone.ZoneID)) return;

        if (currentStep < correctPath.Count)
        {
            Debug.Log($"Zone stepped: {zone.ZoneID}, expecting: {correctPath[currentStep]}");
            if (zone.ZoneID == correctPath[currentStep])
            {
                zone.FlashFeedback(true);
                solvedZoneIDs.Add(zone.ZoneID);
                currentStep++;
                Debug.Log($"Correct step {currentStep}/{correctPath.Count}");
            }
            else
            {
                zone.FlashFeedback(false);
                Debug.Log("wrong step");
                ResetPuzzle();
                StartCoroutine(ReturnToCheckpoint());
            }

            if (currentStep >= correctPath.Count)
            {
                Debug.Log("Puzzle Completed!");
                canStep = false;
                completed = true;
            }
        }
    }
    public void ClearLastZone()
    {
        lastZoneStepped = null;
    }

    private IEnumerator ShowCorrectPath()
    {
        Debug.Log("Showing correct path...");
        foreach (int id in correctPath)
        {
            PuzzleZone zone = zones.Find(z => z.ZoneID == id);
            if (zone)
            {
                zone.Highlight(2.0f);
                yield return new WaitForSeconds(2.1f);
            }
        }
    }

    private IEnumerator ReturnToCheckpoint()
    {
        if (!isLoadingFromSave)
            yield return new WaitForSeconds(0.5f);

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player && CheckpointManager.instance)
        {
            var cpField = typeof(CheckpointManager).GetField("activeCheckpoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Checkpoint activeCP = cpField?.GetValue(CheckpointManager.instance) as Checkpoint;

            if (activeCP != null)
            {
                player.transform.position = activeCP.transform.position;

                if (FindObjectOfType<NewCameraController>() is NewCameraController cam)
                {
                    cam.transform.position = new Vector3(
                        activeCP.transform.position.x,
                        activeCP.transform.position.y,
                        cam.transform.position.z
                    );
                }
                PlayerParty.instance.ResetPartyPositions(activeCP.transform.position);
                ResetPuzzle();
            }
        }
        if (player != null)
        {
            PlayerInput playerInput = player.GetComponent<PlayerInput>();
            if (playerInput != null)
                playerInput.enabled = true;
        }
        ResetPuzzle();
    }

    private void ResetPuzzle(bool respawnToCheckpoint = false)
    {
        solvedZoneIDs.Clear();
        currentStep = 0;
        canStep = false;

        bool wasStarted = puzzleStarted;
        puzzleStarted = false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null)
            playerInput.enabled = true;

        if (respawnToCheckpoint && wasStarted)
            StartCoroutine(ReturnToCheckpoint());
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    public void LoadData(GameData data)
    {
        isLoadingFromSave = true;
        StartCoroutine(LoadPuzzleState(data));
    }

    private IEnumerator LoadPuzzleState(GameData data)
    {
        yield return null;

        if (data.pathPuzzleCompleted)
        {
            canStep = false;
            puzzleStarted = true;
            completed = true;
        }
        else
        {
            ResetPuzzle(true);

            puzzleStarted = false;
            completed = false;
        }

        isLoadingFromSave = false;
    }


    public void SaveData(ref GameData data)
    {
        data.pathPuzzleCompleted = completed;
    }

}
