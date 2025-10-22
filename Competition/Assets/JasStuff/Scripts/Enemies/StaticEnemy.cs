using UnityEngine;

public class StaticEnemy : EnemyBase
{
    private Vector2 originalPosition;
    [SerializeField] private Vector2 startingFacingDir = new Vector2(0, -1);
    private Vector2 originalFacingDir;


    protected override void Start()
    {
        base.Start();

        originalPosition = transform.position;
        originalFacingDir = startingFacingDir.normalized;

        if (anim)
        {
            anim.SetFloat("moveX", originalFacingDir.x);
            anim.SetFloat("moveY", originalFacingDir.y);
        }

        lastMoveDir = originalFacingDir;
        UpdateHitboxDirection();
    }

    protected override void Patrol()
    {
        aiPath.canMove = true;
        aiPath.destination = originalPosition;
        UpdateAnim();

        if (Vector2.Distance(transform.position, originalPosition) < 0.2f)
        {
            aiPath.canMove = false;
            enemyStates = EnemyStates.Idle;

            anim.SetFloat("moveX", originalFacingDir.x);
            anim.SetFloat("moveY", originalFacingDir.y);
            lastMoveDir = originalFacingDir;
            UpdateHitboxDirection();
        }
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
}
