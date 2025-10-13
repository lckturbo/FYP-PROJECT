using UnityEngine;

public class BasicEnemy : EnemyBase
{
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
