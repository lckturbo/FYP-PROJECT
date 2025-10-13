using UnityEngine;

public class BasicEnemy : EnemyBase
{
    [SerializeField] private WayPointArea patrolArea;
    [SerializeField] private PatrolDirection patrolDir = PatrolDirection.Forward;

    protected override void Start()
    {
        base.Start();

        patrolArea = WaypointManager.instance.GetAreaByID(areaID);
        if (patrolArea)
        {
            waypoints = patrolArea.GetWaypoints();
            if (waypoints.Count > 0)
            {
                currId = patrolDir == PatrolDirection.Forward ? 0 : waypoints.Count - 1;
                dirStep = patrolDir == PatrolDirection.Forward ? 1 : -1;
                currTarget = waypoints[currId].position;
            }
        }
    }
    protected override void Idle()
    {
        base.Idle();

        idleTimer -= Time.deltaTime;
        if (idleTimer < 0 && waypoints != null && waypoints.Count > 0)
        {
            enemyStates = EnemyStates.Patrol;
            idleTimer = 2;
        }
    }
}
