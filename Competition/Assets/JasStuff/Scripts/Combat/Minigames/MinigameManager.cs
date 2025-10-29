using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager instance;
    [SerializeField] private GameObject minigameParent;

    public enum ResultType
    {
        Fail,
        Success,
        Perfect
    }

    private bool isMinigameActive = false;

    // For collecting triggers in the same frame
    private int lastTriggerFrame = -1;
    private List<(string id, Action<ResultType> callback)> frameTriggers = new();
    private bool isSelecting = false;

    private void Awake()
    {
        if (!instance) instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Called by animation events via MinigameController.
    /// </summary>
    public void TriggerMinigameFromAnimation(string id, Action<ResultType> onComplete)
    {
        // Collect all triggers within the same frame
        if (Time.frameCount != lastTriggerFrame)
        {
            lastTriggerFrame = Time.frameCount;
            frameTriggers.Clear();
        }

        frameTriggers.Add((id, onComplete));

        // Start selection coroutine only once per frame
        if (!isSelecting)
            StartCoroutine(SelectAndStartRandomMinigame());
    }

    private IEnumerator SelectAndStartRandomMinigame()
    {
        isSelecting = true;
        yield return new WaitForEndOfFrame(); // wait for all animation events this frame

        if (frameTriggers.Count == 0)
        {
            isSelecting = false;
            yield break;
        }

        // Pick a random one from this frame’s triggers
        int randomIndex = UnityEngine.Random.Range(0, frameTriggers.Count);
        var (chosenID, chosenCallback) = frameTriggers[randomIndex];
        Debug.Log($"[MINIGAME] Randomly selected '{chosenID}' from {frameTriggers.Count} triggers this frame.");

        // Start the chosen minigame
        yield return RunMinigameInternal(chosenID, chosenCallback);

        frameTriggers.Clear();
        isSelecting = false;
    }

    private IEnumerator RunMinigameInternal(string id, Action<ResultType> onComplete)
    {
        if (isMinigameActive)
        {
            Debug.Log($"[MINIGAME] '{id}' ignored — another minigame is already running.");
            yield break;
        }

        isMinigameActive = true;
        Debug.Log($"[MINIGAME] Starting {id}...");

        GameObject minigamePrefab = Resources.Load<GameObject>($"Minigames/{id}");
        if (minigamePrefab == null)
        {
            Debug.LogWarning($"[MINIGAME] No prefab found for ID {id}");
            isMinigameActive = false;
            onComplete?.Invoke(ResultType.Fail);
            yield break;
        }

        GameObject instance = Instantiate(minigamePrefab, minigameParent.transform);
        var minigame = instance.GetComponent<BaseMinigame>();

        if (minigame != null)
        {
            yield return minigame.Run();
            onComplete?.Invoke(minigame.Result);
        }

        Destroy(instance);
        isMinigameActive = false;
    }
}
