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

        Vector3 pos = spawnPoint ? spawnPoint.position : Vector3.zero;
        Quaternion rot = spawnPoint ? spawnPoint.rotation : Quaternion.identity;

        var go = Instantiate(prefab, pos, rot);
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

    //private void Awake()
    //{
    //    if (!selectedStore || !selectedStore.definition)
    //    {
    //        Debug.LogError("PlayerSpawner: No SelectedCharacter/definition assigned.");
    //        return;
    //    }

    //    var def = selectedStore.definition;
    //    var prefab = def.playerPrefab;
    //    if (!prefab)
    //    {
    //        Debug.LogError($"PlayerSpawner: '{def.displayName}' has no playerPrefab assigned.");
    //        return;
    //    }

    //    Vector3 pos;
    //    pos = spawnPoint ? spawnPoint.position : Vector3.zero; // new game
    //    var rot = spawnPoint ? spawnPoint.rotation : Quaternion.identity;

    //    var go = Instantiate(prefab, pos, rot);
    //    go.name = $"Player_{def.displayName}";

    //    // Apply stats
    //    var stats = def.stats; // NewCharacterStats : BaseStats
    //    if (stats != null)
    //    {
    //        go.GetComponentInChildren<NewPlayerMovement>()?.ApplyStats(stats);
    //        go.GetComponentInChildren<NewHealth>()?.ApplyStats(stats);
    //    }

    //    var camCtrl = Camera.main ? Camera.main.GetComponent<NewCameraController>() : null;
    //    if (!camCtrl)
    //        camCtrl = FindFirstObjectByType<NewCameraController>();

    //    if (camCtrl)
    //    {
    //        camCtrl.target = go.transform;
    //    }
    //    else
    //    {
    //        Debug.LogWarning("PlayerSpawner: No NewCameraController found in scene (tag your camera MainCamera or add the component).");
    //    }
    //}
}
