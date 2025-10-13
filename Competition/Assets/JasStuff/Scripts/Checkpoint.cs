using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private int checkpointID;
    public int GetID() => checkpointID;
    [SerializeField] private bool isActive = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ActivateCheckpoint();
        }
    }

    public void ActivateCheckpoint()
    {
        if (isActive) return;

        isActive = true;
        CheckpointManager.instance.SetActiveCheckpoint(this);
    }
}