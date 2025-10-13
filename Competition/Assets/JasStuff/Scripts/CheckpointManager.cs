using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    private void Start()
    {
        checkpointList = FindObjectsOfType<Checkpoint>().ToList();
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
        if (data.hasCheckpoint)
        {
            activeCheckpoint = checkpointList.Find(c => c.GetID() == data.lastCheckpointID);
        }
    }

    public void SaveData(ref GameData data)
    {
        if (activeCheckpoint != null)
        {
            data.hasCheckpoint = true;
            data.lastCheckpointID = activeCheckpoint.GetID();
        }
        else
        {
            data.hasCheckpoint = false;
            data.lastCheckpointID = 0;
        }
    }

    public Checkpoint GetCheckpointByID(int id)
    {
        return checkpointList.FirstOrDefault(c => c.GetID() == id);
    }
}
