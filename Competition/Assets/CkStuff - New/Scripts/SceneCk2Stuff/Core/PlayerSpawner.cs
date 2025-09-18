using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private SelectedCharacter selectedStore;
    [SerializeField] private Transform spawnPoint;

    // Optional: others can subscribe if they also need the player Transform
    public static System.Action<Transform> OnPlayerSpawned;

    private void Awake()
    {
        if (!selectedStore || !selectedStore.definition)
        {
            Debug.LogError("PlayerSpawner: No SelectedCharacter/definition assigned.");
            return;
        }

        var def = selectedStore.definition;
        var prefab = def.playerPrefab;
        if (!prefab)
        {
            Debug.LogError($"PlayerSpawner: '{def.displayName}' has no playerPrefab assigned.");
            return;
        }

        var pos = spawnPoint ? spawnPoint.position : Vector3.zero;
        var rot = spawnPoint ? spawnPoint.rotation : Quaternion.identity;

        var go = Instantiate(prefab, pos, rot);
        go.name = $"Player_{def.displayName}";

        // Apply stats
        var stats = def.stats; // NewCharacterStats : BaseStats
        if (stats != null)
        {
            go.GetComponentInChildren<NewPlayerMovement>()?.ApplyStats(stats);
            go.GetComponentInChildren<NewHealth>()?.ApplyStats(stats);
        }

        // ---- Hook the camera target here ----
        var camCtrl = Camera.main ? Camera.main.GetComponent<NewCameraController>() : null;
        if (!camCtrl)
            camCtrl = FindFirstObjectByType<NewCameraController>(); // fallback if not tagged MainCamera

        if (camCtrl)
        {
            camCtrl.target = go.transform;
        }
        else
        {
            Debug.LogWarning("PlayerSpawner: No NewCameraController found in scene (tag your camera MainCamera or add the component).");
        }

        OnPlayerSpawned?.Invoke(go.transform);
    }
}
