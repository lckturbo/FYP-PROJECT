using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PathPuzzle : MonoBehaviour
{
    [SerializeField] private List<int> correctPath;
    [SerializeField] private Transform cameraFocusPoint;
    [SerializeField] private float cameraPanDuration = 0.5f;
    [SerializeField] private float cameraHoldDuration = 1.0f;

    private int currentStep = 0;
    private bool canStep = false;

    private List<PuzzleZone> zones;
    private NewCameraController cam;
    //private PlayerInput playerInput;

    private bool puzzleStarted = false;

    private void Start()
    {
        //playerInput = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInput>();
        zones = new List<PuzzleZone>(FindObjectsOfType<PuzzleZone>());
        cam = FindObjectOfType<NewCameraController>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !puzzleStarted)
        {
            puzzleStarted = true;
            StartCoroutine(StartPuzzleSequence());
        }
    }

    private IEnumerator StartPuzzleSequence()
    {
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
    }

    public void OnZoneStepped(PuzzleZone zone)
    {
        if (!canStep) return;

        if (zone.ZoneID == correctPath[currentStep])
        {
            zone.FlashFeedback(true);
            currentStep++;
            Debug.Log($"Correct step {currentStep}/{correctPath.Count}");

            if (currentStep >= correctPath.Count)
            {
                Debug.Log("Puzzle Completed!");
                canStep = false;
                // TODO: open door, trigger effect
            }
        }
        else
        {
            zone.FlashFeedback(false);

            currentStep = 0;
            canStep = false;

            StartCoroutine(ReturnToCheckpoint());
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

        canStep = true;
        Debug.Log("Player may now step.");
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

        yield return new WaitForSeconds(0.5f);
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
