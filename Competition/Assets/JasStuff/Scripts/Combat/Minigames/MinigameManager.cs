using System;
using System.Collections;
using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager instance;
    public enum ResultType
    {
        Fail, 
        Success, 
        Perfect
    }

    private void Awake()
    {
        if (!instance) instance = this;
        else Destroy(gameObject);
    }
    public void StartMinigame(string id, Action<ResultType> onComplete)
    {
        StartCoroutine(RunMinigame(id, onComplete));
    }
    public IEnumerator RunMinigame(string id, Action<ResultType> onComplete)
    {
        Debug.Log($"[MINIGAME] Starting {id}...");
        GameObject minigamePrefab = Resources.Load<GameObject>($"Minigames/{id}");
        if (minigamePrefab == null)
        {
            Debug.LogWarning($"[MINIGAME] No prefab found for ID {id}");
            onComplete?.Invoke(ResultType.Fail);
            yield break;
        }

        GameObject instance = Instantiate(minigamePrefab);
        var minigame = instance.GetComponent<BaseMinigame>();
        if (minigame != null)
        {
            yield return minigame.Run();
            onComplete?.Invoke(minigame.Result);
        }

        Destroy(instance);
    }
}
