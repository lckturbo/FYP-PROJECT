using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private int checkpointID;
    private bool isActive;
    private bool isLocked;
    public int GetID() => checkpointID;
    public bool IsActive() => isActive;
    public bool IsLocked() => isLocked;

    private void Awake()
    {
        if (CheckpointManager.instance != null)
            CheckpointManager.instance.RegisterCheckpoint(this);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            Activate();
    }
    public void Activate()
    {
        if (isLocked) return;

        isActive = true;
        isLocked = true;

        CheckpointManager.instance.SetActiveCheckpoint(this);

        Debug.Log($"Checkpoint {checkpointID} activated and locked.");
    }

    public void Deactivate()
    {
        isActive = false;
    }

    public void Lock()
    {
        isLocked = true;
    }

    public void Unlock()
    {
        isLocked = false;
    }
}