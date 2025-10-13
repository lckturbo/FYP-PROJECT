using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour, IDataPersistence
{
    [SerializeField] private SelectedCharacter selectedStore;
    [SerializeField] private Transform spawnPoint;

    private Vector2 position;
    public static event Action<Transform> OnPlayerSpawned;

    public void LoadData(GameData data)
    {
        selectedStore.index = data.selectedCharacterIndex;
        selectedStore.RestoreFromIndex(data.selectedCharacterIndex);
        position = data.playerPosition;
    }

    public void SaveData(ref GameData data) { }

    private void Start()
    {
        if (!selectedStore || !selectedStore.definition)
            return;

        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        var def = selectedStore.definition;
        var prefab = def.playerPrefab;
        if (!prefab) return;

        var data = SaveLoadSystem.instance.GetGameData();
        if (data != null)
        {
            if (data.hasEncounterPosition)
            {
                position = data.lastEncounterPosition;
                Debug.Log("[PlayerSpawner] Spawning at last encounter position.");
                // Reset so it doesn't trigger again after respawn
                data.hasEncounterPosition = false;
            }
            else if (data.hasCheckpoint)
            {
                var checkpoint = CheckpointManager.instance.GetCheckpointByID(data.lastCheckpointID);
                if (checkpoint)
                {
                    position = checkpoint.transform.position;
                    Debug.Log($"[PlayerSpawner] Spawning at checkpoint: {checkpoint.GetID()}");
                }
                else
                {
                    position = spawnPoint ? (Vector2)spawnPoint.position : Vector2.zero;
                    Debug.Log("[PlayerSpawner] Checkpoint not found, using default spawn point.");
                }
            }
            else
            {
                position = spawnPoint ? (Vector2)spawnPoint.position : Vector2.zero;
                Debug.Log("[PlayerSpawner] No saved or checkpoint data, using default spawn.");
            }
        }


        Quaternion rot = spawnPoint ? spawnPoint.rotation : Quaternion.identity;
        var go = Instantiate(prefab, position, rot);
        go.name = $"Player_{def.displayName}";

        SetLayerRecursively(go, LayerMask.NameToLayer("Player"));

        var applier = go.GetComponent<PlayerLevelApplier>();
        if (applier != null)
        {
            int partyLevel = 1;
            if (PartyLevelSystem.Instance != null && PartyLevelSystem.Instance.levelSystem != null)
                partyLevel = PartyLevelSystem.Instance.levelSystem.level;

            applier.ApplyForLevel(partyLevel);
            Debug.Log($"[PlayerSpawner] Applied party level {partyLevel} to player via PlayerLevelApplier.");
        }
        else
        {
            Debug.LogWarning("[PlayerSpawner] PlayerLevelApplier not found on player prefab. Stats will not scale with level.");
        }

        var camCtrl = Camera.main ? Camera.main.GetComponent<NewCameraController>() : FindFirstObjectByType<NewCameraController>();
        if (camCtrl) camCtrl.target = go.transform;

        PlayerParty.instance.SetupParty(def, new List<NewCharacterDefinition>());
        var fullParty = PlayerParty.instance.GetFullParty();

        Transform lastTarget = go.transform;
        int index = 0;

        foreach (var memberDef in fullParty)
        {
            if (memberDef == def) continue;
            if (!memberDef.playerPrefab) continue;

            Vector3 spawnPos = go.transform.position + new Vector3(-1.5f * (index + 1), 0f, 0f);
            var followerObj = Instantiate(memberDef.playerPrefab, spawnPos, rot);
            followerObj.name = $"Follower_{memberDef.displayName}";

            SetLayerRecursively(followerObj, LayerMask.NameToLayer("Ally"));

            followerObj.GetComponentInChildren<PlayerHeldItem>().handPoint.gameObject.SetActive(false);
            Destroy(followerObj.GetComponentInChildren<NewPlayerMovement>());
            Destroy(followerObj.GetComponentInChildren<PlayerHeldItem>());
            Destroy(followerObj.GetComponentInChildren<PlayerAttack>());

            var follower = followerObj.AddComponent<PartyFollower>();
            follower.SetTarget(lastTarget);
            lastTarget = followerObj.transform;
            index++;
        }

        OnPlayerSpawned?.Invoke(go.transform);
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}
