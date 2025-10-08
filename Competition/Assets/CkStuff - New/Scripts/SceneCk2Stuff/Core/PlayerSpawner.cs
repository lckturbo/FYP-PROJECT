using System;
using UnityEngine;
public class PlayerSpawner : MonoBehaviour, IDataPersistence
{
    [SerializeField] private SelectedCharacter selectedStore;
    [SerializeField] private Transform spawnPoint;
    private Vector2 position;
    public static event Action<Transform> OnPlayerSpawned; // for enemy
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

        // JAS ADDED -> load player position //
        var data = SaveLoadSystem.instance.GetGameData();
        if (data != null && data.hasSavedPosition)
        {
            position = data.playerPosition;
            Debug.Log("loaded position -> from PlayerSpawner");
        }
        else
            position = spawnPoint ? spawnPoint.position : Vector2.zero;
       
        Quaternion rot = spawnPoint ? spawnPoint.rotation : Quaternion.identity;
        var go = Instantiate(prefab, position, rot);
        go.name = $"Player_{def.displayName}";

        SetLayerRecursively(go, LayerMask.NameToLayer("Player"));
        var playerAnimator = go.GetComponentInChildren<Animator>();

        Vector2 leaderFacingDir = Vector2.down;

        if (playerAnimator)
        {
            float x = playerAnimator.GetFloat("moveX");
            float y = playerAnimator.GetFloat("moveY");

            if (x == 0f && y == 0f)
            {
                playerAnimator.SetFloat("moveX", 0f);
                playerAnimator.SetFloat("moveY", -1f);
                playerAnimator.SetBool("moving", false);
            }
            else
            {
                leaderFacingDir = new Vector2(x, y);
            }
        }

        var stats = def.stats;
        if (stats != null)
        {
            go.GetComponentInChildren<NewPlayerMovement>()?.ApplyStats(stats);
            go.GetComponentInChildren<NewHealth>()?.ApplyStats(stats);
        }
        var camCtrl = Camera.main ? Camera.main.GetComponent<NewCameraController>() : FindFirstObjectByType<NewCameraController>();
        if (camCtrl) camCtrl.target = go.transform;

        // JAS ADDED -> allies set up //
        PlayerParty.instance.SetupParty(def, new System.Collections.Generic.List<NewCharacterDefinition>());
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
            follower.SetTarget(lastTarget, leaderFacingDir);
            follower.FaceSameDirectionAs(leaderFacingDir);

            lastTarget = followerObj.transform;
            index++;
        }
        OnPlayerSpawned?.Invoke(go.transform);
    }
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}