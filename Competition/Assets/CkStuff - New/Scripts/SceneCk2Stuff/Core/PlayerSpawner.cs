using System;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour, IDataPersistence
{
    [SerializeField] private SelectedCharacter selectedStore;
    [SerializeField] private Transform spawnPoint;

    public static event Action<Transform> OnPlayerSpawned; // for enemy

    public void LoadData(GameData data)
    {
        selectedStore.index = data.selectedCharacterIndex;
        selectedStore.RestoreFromIndex(data.selectedCharacterIndex);
    }

    public void SaveData(ref GameData data) { }

    private void Start()
    {
        if (!selectedStore || !selectedStore.definition)
        {
            Debug.LogError("PlayerSpawner: No SelectedCharacter/definition assigned after load.");
            return;
        }

        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        var def = selectedStore.definition;
        var prefab = def.playerPrefab;
        if (!prefab)
        {
            Debug.LogError($"PlayerSpawner: '{def.displayName}' has no playerPrefab assigned.");
            return;
        }

        // JAS ADDED -> load player position //
        Vector2 position;
        var data = SaveLoadSystem.instance.GetGameData();
        if (data != null && data.playerPosition != Vector2.zero)
            position = data.playerPosition;
        else
            position = spawnPoint ? spawnPoint.position : Vector2.zero;
        ///////////////

        Quaternion rot = spawnPoint ? spawnPoint.rotation : Quaternion.identity;

        var go = Instantiate(prefab, position, rot);
        go.name = $"Player_{def.displayName}";

        var stats = def.stats;
        if (stats != null)
        {
            go.GetComponentInChildren<NewPlayerMovement>()?.ApplyStats(stats);
            go.GetComponentInChildren<NewHealth>()?.ApplyStats(stats);
        }

        var camCtrl = Camera.main ? Camera.main.GetComponent<NewCameraController>() : FindFirstObjectByType<NewCameraController>();
        if (camCtrl) camCtrl.target = go.transform;

        OnPlayerSpawned?.Invoke(go.transform);
    }
}
