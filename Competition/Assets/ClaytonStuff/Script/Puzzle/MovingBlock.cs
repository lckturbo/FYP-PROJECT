using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MovingBlock : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTime = 1f;

    [Header("Player Reset Settings")]
    [SerializeField] private int waypointAreaID = 0; // Which area to send player to

    private int currentIndex = 0;
    private bool waiting = false;

    private void Start()
    {
        if (waypoints.Length == 0)
        {
            Debug.LogWarning($"{name}: No waypoints assigned!");
            enabled = false;
            return;
        }

        transform.position = waypoints[0].position;
    }

    private void Update()
    {
        if (waiting || waypoints.Length == 0) return;

        Transform target = waypoints[currentIndex];
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
        if (currentIndex >= waypoints.Length)
            currentIndex = 0;

        waiting = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Find waypoint area by ID
            WayPointArea area = WaypointManager.instance?.GetAreaByID(waypointAreaID);
            if (area == null)
            {
                Debug.LogWarning($"{name}: No WayPointArea found with ID {waypointAreaID}");
                return;
            }

            // Get first waypoint from that area
            var waypoints = area.GetWaypoints();
            if (waypoints.Count > 0)
            {
                // Teleport player to first waypoint
                collision.transform.position = waypoints[0].position;
                Debug.Log($"Player reset to waypoint: {waypoints[0].name}");
            }
            else
            {
                Debug.LogWarning($"{name}: WayPointArea {waypointAreaID} has no waypoints!");
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.1f);

            if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
}
