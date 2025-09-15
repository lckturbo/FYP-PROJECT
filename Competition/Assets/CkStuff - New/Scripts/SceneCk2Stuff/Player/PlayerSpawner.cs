using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private SelectedCharacter selectedStore;
    [SerializeField] private Transform spawnPoint;

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

        var stats = def.stats; // NewCharacterStats : BaseStats
        if (stats != null)
        {
            go.GetComponentInChildren<NewPlayerMovement>()?.ApplyStats(stats);
            go.GetComponentInChildren<NewHealth>()?.ApplyStats(stats);
        }
    }
}
