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

    public void SetActiveCheckpoint(Checkpoint newCheckpoint)
    {
        activeCheckpoint = newCheckpoint;
        SaveLoadSystem.instance.SaveGame();
    }

    public void LoadData(GameData data)
    {
        if (data.hasCheckpoint)
        {
            activeCheckpoint = checkpointList.FirstOrDefault(c => c.GetID() == data.lastCheckpointID);
        }
    }

    public void SaveData(ref GameData data)
    {
        if (activeCheckpoint)
        {
            data.lastCheckpointID = activeCheckpoint.GetID();
            data.hasCheckpoint = true;
        }
        else
        {
            data.hasCheckpoint = false;
        }
    }

    public Checkpoint GetCheckpointByID(int id)
    {
        return checkpointList.FirstOrDefault(c => c.GetID() == id);
    }
}
