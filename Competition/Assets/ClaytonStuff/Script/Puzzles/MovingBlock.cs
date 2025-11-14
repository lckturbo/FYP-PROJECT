using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MovingBlock : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Transform[] checkpoints;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTime = 1f;

    [Header("Rotation Settings")]
    [SerializeField] private float spinSpeed = 180f;

    [Header("Player Reset Settings")]
    [SerializeField] private int checkpointID = 0;

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

        // Rotate continuously
        transform.Rotate(Vector3.forward * spinSpeed * Time.deltaTime);

        if (waiting) return;

        Transform target = checkpoints[currentIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
            StartCoroutine(WaitAndMoveNext());

        //AudioManager.instance.PlayLoopingSFXAtObject("toy-chainsaw-60479", gameObject, 0.5f);
    }

    private System.Collections.IEnumerator WaitAndMoveNext()
    {
        waiting = true;
        yield return new WaitForSeconds(waitTime);

        currentIndex = (currentIndex + 1) % checkpoints.Length;
        waiting = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int allyLayer = LayerMask.NameToLayer("Ally");

        bool isPlayer = collision.gameObject.layer == playerLayer;
        bool isAlly = collision.gameObject.layer == allyLayer;

        if (!isPlayer && !isAlly)
            return;

        // Find the player by layer
        GameObject player = FindObjectOfType<NewPlayerMovement>()?.gameObject;
        if (player == null)
        {
            Debug.LogWarning("No player found in scene.");
            return;
        }

        // Find checkpoint
        Checkpoint targetCheckpoint = CheckpointManager.instance?.GetCheckpointByID(checkpointID);
        if (targetCheckpoint == null)
        {
            Debug.LogWarning($"{name}: No checkpoint found with ID {checkpointID}");
            return;
        }

        // Move player to checkpoint
        player.transform.position = targetCheckpoint.transform.position;
        Debug.Log($"Player reset to checkpoint ID {checkpointID}");

        // Move all allies to the player’s new position
        BringAlliesToLeader(player.transform, allyLayer);
    }

    private void BringAlliesToLeader(Transform leader, int allyLayer)
    {
        foreach (GameObject ally in FindObjectsOfType<GameObject>())
        {
            if (ally.layer == allyLayer)
            {
                ally.transform.position = leader.position;
                Debug.Log($"Ally {ally.name} moved to leader position");
            }
        }
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
