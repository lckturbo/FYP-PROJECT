using UnityEngine;

public class StaticEnemy : EnemyBase
{
    private Vector2 originalPosition;
    [SerializeField] private Vector2 startingFacingDir = new Vector2(0, -1);
    private Vector2 originalFacingDir;

    [Header("Battle Dialogue")]
    public DialogueData attackDialogue;   // <-- assign per enemy in Inspector


    protected override void Start()
    {
        base.Start();

        originalPosition = transform.position;

        var nn = AstarPath.active.GetNearest(originalPosition);
        originalPosition = nn.position;

        originalFacingDir = startingFacingDir.normalized;

        if (anim)
        {
            anim.SetFloat("moveX", originalFacingDir.x);
            anim.SetFloat("moveY", originalFacingDir.y);
        }

        lastMoveDir = originalFacingDir;
        UpdateHitboxDirection();
    }

    protected override void Idle()
    {
        base.Idle();
        aiPath.canMove = false;
        UpdateAnim();

        if (CanSeePlayer())
        {

            //aiPath.isStopped = true;
            //aiPath.canMove = false;
            //rb2d.velocity = Vector2.zero;


            enemyStates = EnemyStates.Alert;
            return;
        }
    }
    protected override void Patrol()
    {
        aiPath.canMove = true;
        aiPath.destination = originalPosition;
        UpdateAnim();

        if (CanSeePlayer())
        {
            //aiPath.isStopped = true;
            //aiPath.canMove = false;
            //rb2d.velocity = Vector2.zero;

            //isAlerting = true;
            enemyStates = EnemyStates.Alert;
            return;
        }

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

    //protected override void StateMachine()
    //{
    //    switch (enemyStates)
    //    {
    //        case EnemyStates.Idle:
    //            Idle();
    //            break;
    //        case EnemyStates.Patrol:
    //            Patrol();
    //            break;
    //        case EnemyStates.Alert:
    //            Alert();
    //            break;
    //        case EnemyStates.Chase:
    //            Chase();
    //            break;
    //        case EnemyStates.Attack:
    //            Attack(); // trigger battle scene
    //            break;
    //    }
    //}
}
