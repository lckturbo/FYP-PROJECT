using UnityEngine;

public class StaticEnemy : EnemyBase
{
    private Vector2 originalPosition;

    protected override void Start()
    {
        base.Start();
        originalPosition = transform.position;
    }
    protected override void Chase()
    {
        if (!player) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > enemyStats.chaseRange)
        {
            enemyStates = EnemyStates.Patrol;
            return;
        }

        aiPath.canMove = true;
        aiPath.destination = player.position;

        if (dist < enemyStats.atkRange && !isAttacking && attackCooldownTimer <= 0f)
            enemyStates = EnemyStates.Attack;

        UpdateAnim();
    }
    protected override void Patrol()
    {
        aiPath.canMove = true;
        aiPath.destination = originalPosition;
        UpdateAnim();

        if (Vector2.Distance(transform.position, originalPosition) < 0.1f)
        {
            aiPath.canMove = false;
            enemyStates = EnemyStates.Idle;
        }
    }
}
