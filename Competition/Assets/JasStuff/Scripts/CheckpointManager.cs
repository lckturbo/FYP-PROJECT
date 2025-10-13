using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour, IDataPersistence
{
    public static CheckpointManager instance;

    private List<Checkpoint> checkpointList;
    private Checkpoint activeCheckpoint;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private void OnEnable()
    {
        checkpointList = new List<Checkpoint>(FindObjectsOfType<Checkpoint>());
        checkpointList = checkpointList.Where(c => c != null).ToList();

        Debug.Log($"[CheckpointManager] Registered {checkpointList.Count} checkpoints in scene {SceneManager.GetActiveScene().name}");
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
                cp.Activate(); // optional: visually reactivate
                Debug.Log($"[CheckpointManager] Restored checkpoint ID {data.lastCheckpointID}");
            }
            else
            {
                Debug.LogWarning($"[CheckpointManager] Could not find checkpoint ID {data.lastCheckpointID}");
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        if (checkpointList == null || checkpointList.Count == 0)
        {
            Debug.Log("[CheckpointManager] No checkpoints in this scene — skipping save.");
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
}
