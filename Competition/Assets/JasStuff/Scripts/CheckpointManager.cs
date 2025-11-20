using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class CheckpointManager : MonoBehaviour, IDataPersistence
{
    public static CheckpointManager instance;

    private List<Checkpoint> checkpointList;
    private Checkpoint activeCheckpoint;
    private int cheatIndex = 0;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        checkpointList = FindObjectsOfType<Checkpoint>().OrderBy(c => c.GetID()).ToList();
    }
    //private void Update()
    //{
    //    if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C))
    //    {
    //        TeleportToNextCheckpoint();
    //    }
    //}
    private void TeleportToNextCheckpoint()
    {
        if (checkpointList == null || checkpointList.Count == 0)
            return;

        cheatIndex++;
        if (cheatIndex >= checkpointList.Count)
            cheatIndex = 0;

        activeCheckpoint = checkpointList[cheatIndex];
        Debug.Log("[CheckpointManager] Cheat teleport to checkpoint: " + activeCheckpoint.GetID());
        ReturnToCheckpoint();
    }
    public void RegisterCheckpoint(Checkpoint checkpoint)
    {
        if (checkpointList == null)
            checkpointList = new List<Checkpoint>();

        if (!checkpointList.Contains(checkpoint))
            checkpointList.Add(checkpoint);
    }
    public void SetActiveCheckpoint(Checkpoint newCheckpoint)
    {
        if (activeCheckpoint == newCheckpoint)
            return;

        foreach (var checkpoint in checkpointList)
        {
            checkpoint.Deactivate();
        }

        activeCheckpoint = newCheckpoint;
        SaveLoadSystem.instance.SaveGame();
    }


    public void LoadData(GameData data)
    {
        checkpointList = FindObjectsOfType<Checkpoint>().ToList();

        if (data.hasCheckpoint)
        {
            var cp = checkpointList.Find(c => c.GetID() == data.lastCheckpointID);
            if (cp != null)
            {
                activeCheckpoint = cp;
                cp.Activate(); 
               // Debug.Log($"[CheckpointManager] Restored checkpoint ID {data.lastCheckpointID}");
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        if (checkpointList == null || checkpointList.Count == 0)
        {
            //Debug.Log("[CheckpointManager] No checkpoints in this scene — skipping save.");
            return;
        }

        if (activeCheckpoint != null)
        {
            data.hasCheckpoint = true;
            data.lastCheckpointID = activeCheckpoint.GetID();
        }
    }

    public Checkpoint GetCheckpointByID(int id)
    {
        return checkpointList.FirstOrDefault(c => c.GetID() == id);
    }

    public void ClearCheckpoints()
    {
        checkpointList?.Clear();
        activeCheckpoint = null;
    }
    public void ReturnToCheckpoint()
    {
        if (activeCheckpoint == null)
            return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
            return;

        Vector3 cpPos = activeCheckpoint.transform.position;
        playerObj.transform.position = cpPos;

        if (PlayerParty.instance != null)
            PlayerParty.instance.ResetPartyPositions(cpPos);

        //var fogs = FindObjectsOfType<FoggedTrigger>();

        //foreach (var fog in fogs)
        //{
        //    if (fog.ContainsPoint(cpPos))
        //    {
        //        fog.ForceClear();
        //    }
        //}
    }


    public Checkpoint GetActiveCheckpoint()
    {
        return activeCheckpoint;
    }

    public int GetCheckpointIndex(Checkpoint checkpoint)
    {
        if (checkpointList == null || checkpoint == null)
            return -1;

        return checkpointList.IndexOf(checkpoint);
    }

}
