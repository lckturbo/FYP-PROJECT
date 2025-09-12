using Pathfinding;
using UnityEngine;

public class NewBasicEnemy : MonoBehaviour
{
    // target -> waypoint manager._waypoints
    private Path _path;
    private int _currWayPoint;
    private bool _reachedPath;

    [SerializeField] private Seeker _seeker;
    [SerializeField] private Rigidbody2D _rb;

    private void Start()
    {
        if (!_rb || !_seeker) return;

        StartPathToNextWaypoint();

    }

    private void StartPathToNextWaypoint()
    {
        if (!WayPointManager.instance) return;

        _currWayPoint %= WayPointManager.instance.GetTotalWayPoints();
        Vector3 target = WayPointManager.instance.GetWayPoint(_currWayPoint);
        _seeker.StartPath(_rb.position, target, OnPathComplete);
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            _path = p;
        }
    }

    private void FixedUpdate()
    {
        if (_path == null || _reachedPath || _path.vectorPath.Count == 0) return;

        Vector2 dir = ((Vector2)_path.vectorPath[0] -_rb.position).normalized;

        if (Vector2.Distance(_rb.position, (Vector2)_path.vectorPath[_path.vectorPath.Count -1])< 0.1f)
        {
            _currWayPoint++;
            StartPathToNextWaypoint();
        }
    }
}
