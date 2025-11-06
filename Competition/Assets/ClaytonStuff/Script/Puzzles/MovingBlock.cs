using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MovingBlock : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Transform[] checkpoints;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTime = 1f;

    [Header("Rotation Settings")]
    [SerializeField] private float spinSpeed = 180f; // ?? Degrees per second

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
        if (checkpoints.Length == 0) return;

        // ?? Always spin
        transform.Rotate(Vector3.forward * spinSpeed * Time.deltaTime);

        if (waiting) return;

        // ?? Move toward the target
        Transform target = checkpoints[currentIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        // ?? Check if reached target
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
        // --- Handle both player and ally ---
        bool isPlayer = collision.gameObject.CompareTag("Player");
        bool isAlly = collision.gameObject.layer == LayerMask.NameToLayer("Ally");

        if (!isPlayer && !isAlly)
            return;

        // Get target checkpoint
        Checkpoint targetCheckpoint = CheckpointManager.instance?.GetCheckpointByID(checkpointID);
        if (targetCheckpoint == null)
        {
            Debug.LogWarning($"{name}: No checkpoint found with ID {checkpointID}");
            return;
        }

        // Teleport back to checkpoint
        collision.transform.position = targetCheckpoint.transform.position;

        if (isPlayer)
            Debug.Log($"Player reset to checkpoint ID {checkpointID}");
        else if (isAlly)
            Debug.Log($"Ally {collision.gameObject.name} reset to checkpoint ID {checkpointID}");
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
