using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MovingBlock : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Transform[] checkpoints;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTime = 1f;

    [Header("Player Reset Settings")]
    [SerializeField] private int checkpointID = 0; // Which checkpoint to send player to

    private int currentIndex = 0;
    private bool waiting = false;

    private void Start()
    {
        if (checkpoints.Length == 0)
        {
            Debug.LogWarning($"{name}: No checkpoints assigned!");
            enabled = false;
            return;
        }

        transform.position = checkpoints[0].position;
    }

    private void Update()
    {
        if (waiting || checkpoints.Length == 0) return;

        Transform target = checkpoints[currentIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            StartCoroutine(WaitAndMoveNext());
        }
    }

    private System.Collections.IEnumerator WaitAndMoveNext()
    {
        waiting = true;
        yield return new WaitForSeconds(waitTime);

        currentIndex++;
        if (currentIndex >= checkpoints.Length)
            currentIndex = 0;

        waiting = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        // Get the checkpoint by ID
        Checkpoint targetCheckpoint = CheckpointManager.instance?.GetCheckpointByID(checkpointID);
        if (targetCheckpoint == null)
        {
            Debug.LogWarning($"{name}: No checkpoint found with ID {checkpointID}");
            return;
        }

        // Move the player to the checkpoint position
        collision.transform.position = targetCheckpoint.transform.position;
        Debug.Log($"Player reset to checkpoint ID {checkpointID}");
    }

    private void OnDrawGizmos()
    {
        if (checkpoints == null || checkpoints.Length < 2) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] == null) continue;
            Gizmos.DrawSphere(checkpoints[i].position, 0.1f);

            if (i < checkpoints.Length - 1 && checkpoints[i + 1] != null)
                Gizmos.DrawLine(checkpoints[i].position, checkpoints[i + 1].position);
        }
    }
}
