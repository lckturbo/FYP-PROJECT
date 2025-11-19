using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour, IDataPersistence
{
    [SerializeField] private CharacterDatabase characterDatabase;

    [SerializeField] private SelectedCharacter selectedStore;
    private List<int> cachedAllyIndices = new List<int>();
    [SerializeField] private Transform spawnPoint;

    private Vector2 position;
    public static event Action<Transform> OnPlayerSpawned;

    public void LoadData(GameData data)
    {
        selectedStore.index = data.selectedCharacterIndex;
        selectedStore.RestoreFromIndex(data.selectedCharacterIndex);
        position = data.playerPosition;

        cachedAllyIndices = new List<int>(data.allyIndices);
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

        // JAS ADDED -> LOAD PLAYER POSITION
        var data = SaveLoadSystem.instance.GetGameData();
        if (data != null)
        {
            if (data.hasSavedPosition)
                position = data.playerPosition;
            else if (data.hasCheckpoint)
            {
                var cp = CheckpointManager.instance.GetCheckpointByID(data.lastCheckpointID);
                if (cp)
                    position = cp.transform.position;
            }
            else
                position = spawnPoint ? (Vector2)spawnPoint.position : Vector2.zero;
        }

        Quaternion rot = spawnPoint ? spawnPoint.rotation : Quaternion.identity;
        var go = Instantiate(prefab, position, rot);
        go.name = $"Player_{def.displayName}";

        var movement = go.GetComponent<NewPlayerMovement>();
        if (movement != null)
        {
            var anim = go.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetFloat("moveX", 0f);
                anim.SetFloat("moveY", -1f);
                anim.SetBool("moving", false);
            }
        }

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

        var allies = new List<NewCharacterDefinition>();

        if (cachedAllyIndices != null && cachedAllyIndices.Count > 0)
        {
            foreach (int allyIndex in cachedAllyIndices)
            {
                if (allyIndex >= 0 && allyIndex < characterDatabase.roster.Length)
                {
                    var allyDef = characterDatabase.GetByIndex(allyIndex);
                    if (allyDef != null) allies.Add(allyDef);
                }
            }
        }

        PlayerParty.instance.SetupParty(def, allies);
        var fullParty = PlayerParty.instance.GetFullParty();

        Transform lastTarget = go.transform;
        int index = 0;

        foreach (var memberDef in fullParty)
        {
            if (memberDef == def) continue;
            if (!memberDef.playerPrefab) continue;

            Vector3 spawnPos = go.transform.position + new Vector3(-1.0f * (index + 1), 0f, 0f);
            var followerObj = Instantiate(memberDef.playerPrefab, spawnPos, rot);
            followerObj.name = $"Follower_{memberDef.displayName}";

            SetLayerRecursively(followerObj, LayerMask.NameToLayer("Ally"));

            followerObj.GetComponentInChildren<PlayerHeldItem>().handPoint.gameObject.SetActive(false);
            followerObj.tag = "Untagged";
            var mv = followerObj.GetComponentInChildren<NewPlayerMovement>();
            if (mv != null)
                mv.enabled = false;
            Destroy(followerObj.GetComponentInChildren<PlayerHeldItem>());
            Destroy(followerObj.GetComponentInChildren<PlayerAttack>());
            Destroy(followerObj.GetComponentInChildren<PlayerBuffHandler>());
            Destroy(followerObj.GetComponentInChildren<PushableBlock>());
            Destroy(followerObj.GetComponentInChildren<StatueController>());
            Destroy(followerObj.GetComponentInChildren<PieceController>());
            Destroy(followerObj.GetComponentInChildren<PlayerPush>());
            Destroy(followerObj.GetComponentInChildren<Collider2D>());

            var follower = followerObj.AddComponent<PartyFollower>();
            follower.followerIndex = index;
            //follower.stepsBehind = 8;

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
