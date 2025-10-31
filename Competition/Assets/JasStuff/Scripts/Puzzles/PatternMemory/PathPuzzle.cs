using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PathPuzzle : MonoBehaviour
{
    [SerializeField] private ToggleableBlock doorToOpen;
    [SerializeField] private List<int> correctPath;
    [SerializeField] private Transform cameraFocusPoint;
    [SerializeField] private float cameraPanDuration = 0.5f;
    [SerializeField] private float cameraHoldDuration = 1.0f;

    [Header("Door Focus")]
    [SerializeField] private Transform doorFocusPoint;
    [SerializeField] private float doorFocusDuration = 0.5f;

    private int currentStep = 0;
    private bool canStep = false;

    private List<PuzzleZone> zones;
    private NewCameraController cam;

    private bool puzzleStarted = false;

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

        if (currentStep < correctPath.Count)
        {
            if (zone.ZoneID == correctPath[currentStep])
            {
                zone.FlashFeedback(true);
                currentStep++;
                Debug.Log($"Correct step {currentStep}/{correctPath.Count}");
            }
            else
            {
                zone.FlashFeedback(false);

                ResetPuzzle();
                StartCoroutine(ReturnToCheckpoint());
            }

            if (currentStep >= correctPath.Count)
            {
                Debug.Log("Puzzle Completed!");
                canStep = false;

                StartCoroutine(HandlePuzzleComplete());
            }
        }
    }

    private IEnumerator HandlePuzzleComplete()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            PlayerInput playerInput = player.GetComponent<PlayerInput>();
            if (playerInput != null)
                playerInput.enabled = false;
        }

        yield return new WaitForSeconds(0.5f);

        if (cam && doorFocusPoint)
        {
            Debug.Log("[PathPuzzle] Panning camera to door...");
            yield return cam.StartCoroutine(cam.PanTo(doorFocusPoint, 0.8f));
        }

        if (doorToOpen != null)
        {
            doorToOpen.Open();
            Debug.Log("[PathPuzzle] Door opened!");
        }

        yield return new WaitForSeconds(doorFocusDuration);

        if (cam)
            yield return cam.StartCoroutine(cam.ReturnToPlayer(0.8f));

        Debug.Log("[PathPuzzle] Puzzle sequence complete.");

        if (player)
        {
            PlayerInput playerInput = player.GetComponent<PlayerInput>();
            if (playerInput != null)
                playerInput.enabled = true;
        }
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
                    cam.ReturnToPlayer(0.2f);

                RespawnPartyAtCheckpoint(activeCP);
            }
            else
            {
                Debug.LogWarning("[PathPuzzle] No active checkpoint found to respawn at!");
            }
        }

        yield return new WaitForSeconds(1.0f);
        Debug.Log("[PathPuzzle] Puzzle reset and ready for retry.");
    }
    private void ResetPuzzle()
    {
        currentStep = 0;
        canStep = false;
        puzzleStarted = false;
    }

    private void RespawnPartyAtCheckpoint(Checkpoint cp)
    {
        DestroyExistingFollowers();

        if (PlayerParty.instance == null)
        {
            Debug.LogWarning("[PathPuzzle] No PlayerParty instance found!");
            return;
        }

        var party = PlayerParty.instance;
        var fullParty = party.GetFullParty();
        var leaderDef = party.GetLeader();
        Quaternion rot = Quaternion.identity;

        Transform lastTarget = GameObject.FindGameObjectWithTag("Player").transform;
        int index = 0;

        foreach (var memberDef in fullParty)
        {
            if (memberDef == leaderDef) continue;
            if (!memberDef.playerPrefab) continue;

            // Spawn followers near checkpoint
            Vector3 spawnPos = cp.transform.position + new Vector3(-1.5f * (index + 1), 0f, 0f);
            var followerObj = Instantiate(memberDef.playerPrefab, spawnPos, rot);
            followerObj.name = $"Follower_{memberDef.displayName}";

            SetLayerRecursively(followerObj, LayerMask.NameToLayer("Ally"));

            // Disable redundant player controls
            Destroy(followerObj.GetComponentInChildren<NewPlayerMovement>());
            Destroy(followerObj.GetComponentInChildren<PlayerHeldItem>());
            Destroy(followerObj.GetComponentInChildren<PlayerAttack>());
            Destroy(followerObj.GetComponentInChildren<PlayerBuffHandler>());

            // Setup as follower
            var follower = followerObj.AddComponent<PartyFollower>();
            follower.SetTarget(lastTarget);

            lastTarget = followerObj.transform;
            index++;
        }
    }

    private void DestroyExistingFollowers()
    {
        PartyFollower[] followers = FindObjectsOfType<PartyFollower>();

        int destroyedCount = 0;
        foreach (var follower in followers)
        {
            if (follower != null)
            {
                Destroy(follower.gameObject);
                destroyedCount++;
            }
        }

        if (destroyedCount > 0)
            Debug.Log($"[PathPuzzle] Destroyed {destroyedCount} old follower(s).");
    }



    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }



}
