using Pathfinding;
using System;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    public enum EnemyStates
    {
        Idle,
        Patrol,
        Attack,
        BattleAttack,
        Chase
        //Death
    }

    [Header("Enemy States")]
    [SerializeField] protected EnemyStates enemyStates;
    [SerializeField] protected EnemyStats enemyStats;
    public EnemyStats GetEnemyStats() => enemyStats;
    protected Transform player;
    [SerializeField] protected Animator animator;

    [Header("Enemy Components")]
    [SerializeField] protected NewHealth health;
    [SerializeField] protected Seeker seeker;
    [SerializeField] protected Rigidbody2D rb2d;
    [SerializeField] protected AIPath aiPath;
    private Path path;
    private int currWP;
    private bool reachedPath;
    private float currIdleTimer;
    private Waypoints targetwp;

    [SerializeField] private float detectionDist;
    [SerializeField] private float sideOffSet; // left/right raycast
    [SerializeField] private float avoidanceStrength;

    //public event Action<GameObject, float> OnDeath;
    public event Action<GameObject, EnemyParty> OnAttackPlayer;
    protected bool inBattle;

    private void OnEnable()
    {
        PlayerSpawner.OnPlayerSpawned += HandlePlayerSpawned;
    }

    private void OnDisable()
    {
        PlayerSpawner.OnPlayerSpawned -= HandlePlayerSpawned;
    }

    private void HandlePlayerSpawned(Transform playerTransform)
    {
        player = playerTransform;
    }

    private void Awake()
    {
        if (!enemyStats || !aiPath || !rb2d || !seeker) return;
        enemyStates = EnemyStates.Idle;
        aiPath.maxSpeed = enemyStats.Speed;
    }

    protected virtual void Start()
    {
        //if (!player) player = GameObject.FindWithTag("Player").transform;
        if (!animator) animator = GetComponent<Animator>();
        if (!health) health = GetComponent<NewHealth>();
        health.ApplyStats(enemyStats);

        currIdleTimer = enemyStats.idleTimer;
    }
    private void Update()
    {
        StateMachine();

        if (Input.GetKeyDown(KeyCode.L))
            health.TakeDamage(10, enemyStats);
    }
    protected virtual void Idle()
    {
        if (animator) animator.Play("IdleBack");
        aiPath.canMove = false;
        rb2d.velocity = Vector2.zero;
        if (CanSeePlayer())
        {
            enemyStates = EnemyStates.Chase;
            return;
        }

        if (!targetwp || !targetwp.isOccupied())
        {
            targetwp = WayPointManager.instance.GetFreeWayPoint();
            //if (targetwp) targetwp.SetOccupied(true);
        }

        currIdleTimer -= Time.deltaTime;
        if (currIdleTimer <= 0 && targetwp)
        {
            enemyStates = EnemyStates.Patrol;
            currIdleTimer = enemyStats.idleTimer;
        }
    }

    protected virtual void Patrol()
    {
        if (!aiPath || !WayPointManager.instance || !targetwp)
            return;

        Vector2 waypoint = targetwp.transform.position;
        Vector2 avoidance = SteeringAvoidance() + Separation();
        Vector2 final = waypoint + avoidance;
        aiPath.canMove = true;
        aiPath.destination = final;

        Vector2 dir = ((Vector2)aiPath.steeringTarget - (Vector2)transform.position).normalized;
        FaceDir(dir);

        if (aiPath.reachedEndOfPath || aiPath.remainingDistance < 0.5f)
        {
            //targetwp.SetOccupied(false);

            foreach (var neighbor in targetwp.nearestWaypoints)
            {
                //if (!neighbor.isOccupied())
                //{
                    targetwp = neighbor;
                    //targetwp.SetOccupied(true);
                    break;
                //}
            }
            enemyStates = EnemyStates.Idle;
        }
        else if (aiPath.velocity.sqrMagnitude < 0.1f)
            aiPath.SearchPath();

        if (CanSeePlayer()) enemyStates = EnemyStates.Chase;
    }

    private Vector2 SteeringAvoidance()
    {
        if (aiPath.velocity.sqrMagnitude <= 0.01f) return Vector2.zero;

        LayerMask enemyLayer = LayerMask.GetMask("Enemy");
        RaycastHit2D closestHit = new RaycastHit2D();
        float minF = float.MaxValue;

        // forward
        RaycastHit2D hit = Physics2D.Raycast(transform.position, aiPath.desiredVelocity.normalized, detectionDist, enemyLayer);
        if (hit && hit.collider.gameObject != gameObject && hit.fraction < minF)
        {
            closestHit = hit;
            minF = hit.fraction;
        }

        // sides
        Vector2 perp = new Vector2(aiPath.desiredVelocity.y, -aiPath.desiredVelocity.x).normalized * sideOffSet;
        RaycastHit2D leftHit = Physics2D.Raycast((Vector2)transform.position - perp, aiPath.desiredVelocity.normalized, detectionDist, enemyLayer);
        if (leftHit && leftHit.collider.gameObject != gameObject && leftHit.fraction < minF)
        {
            closestHit = leftHit;
            minF = leftHit.fraction;
        }
        RaycastHit2D rightHit = Physics2D.Raycast((Vector2)transform.position + perp, aiPath.desiredVelocity.normalized, detectionDist, enemyLayer);
        if (rightHit && rightHit.collider.gameObject != gameObject && rightHit.fraction < minF)
        {
            closestHit = rightHit;
            minF = rightHit.fraction;
        }

        if (!closestHit.collider) return Vector2.zero;

        // steer left or right
        Vector2 toOther = (Vector2)closestHit.collider.transform.position - (Vector2)transform.position;
        float cross = aiPath.desiredVelocity.x * -toOther.y + aiPath.desiredVelocity.y * toOther.x;
        bool steerLeft = cross > 0;

        Vector2 result = steerLeft
            ? new Vector2(-toOther.y, toOther.x)
            : new Vector2(toOther.y, -toOther.x);

        result.Normalize();
        result *= avoidanceStrength;

        return result / Mathf.Max(0.1f, minF);
    }

    private Vector2 Separation()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.6f, LayerMask.GetMask("Enemy"));
        Vector2 force = Vector2.zero;

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            Vector2 diff = (Vector2)transform.position - (Vector2)hit.transform.position;
            float dist = diff.magnitude;
            if (dist > 0)
                force += diff.normalized / dist;
        }

        return force * avoidanceStrength;
    }

    protected virtual void Chase()
    {
        if (!player) return;
        float dist = Vector2.Distance(rb2d.position, player.position);

        if (dist > enemyStats.chaseRange)
        {
            aiPath.destination = rb2d.position;
            enemyStates = EnemyStates.Idle;
            return;
        }

        aiPath.canMove = true;
        aiPath.destination = player.position;

        if (dist < enemyStats.atkRange)
        {
            aiPath.canMove = false;
            enemyStates = EnemyStates.Attack;
        }
    }

    protected virtual void Attack()
    {
        //float dir = player.position.x - transform.position.x;
        //FaceDir(dir);
        //PlayerNearby();

        if (CanSeePlayer()) enemyStates = EnemyStates.Chase;

        // TODO: play "hit" animation (don't need to deduct player's health -> transition to jasBattle scene
        if (!inBattle)
        {
            BattleManager.instance.RegisterEnemy(this);
            OnAttackPlayer.Invoke(player.gameObject, GetComponent<EnemyParty>());
            inBattle = true;
        }
        enemyStates = EnemyStates.Idle;
    }

    protected abstract void BattleAttack();

    protected virtual bool CanSeePlayer()
    {
        if (!player) return false;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > enemyStats.chaseRange)
            return false;

        Vector2 distToPlayer = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, distToPlayer, enemyStats.chaseRange, LayerMask.GetMask("Player"));

        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    // NORMAL -> ALL FSM
    // BATTLE -> IDLE(waiting turn) -> ATTACK
    protected virtual void StateMachine()
    {
        switch (enemyStates)
        {
            case EnemyStates.Idle:
                Idle();
                break;
            case EnemyStates.Patrol:
                Patrol();
                break;
            case EnemyStates.Chase:
                Chase();
                break;
            case EnemyStates.Attack:
                Attack(); // trigger battle scene
                break;
            case EnemyStates.BattleAttack:
                //BattleAttack();
                break;
                //case EnemyStates.Death:
                //    //Death();
                //    break;
        }
    }
    //protected virtual void Death()
    //{
    //    // TODO: ANIMATION
    //    //OnDeath?.Invoke(this.gameObject, deathTime);
    //    //Destroy(gameObject, deathTime);
    //}

    protected void FaceDir(Vector2 dir)
    {
        if (!player || !animator) return;

        Vector2 moveDir = Vector2.zero;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            // move horizontal
            if (dir.x > 0)
            {
                moveDir.x = 1;
                animator.Play("WalkRight");
                sprite.flipX = false;
            }
            else
            {
                moveDir.x = -1;
                animator.Play("WalkRight");
                sprite.flipX = true;
            }
        }
        else
        {
            // move vertical
            if (dir.y > 0)
            {
                moveDir.y = 1;
                animator.Play("WalkFront");
            }
            else
            {
                moveDir.y = -1;
                animator.Play("WalkBack");
            }
        }
    }
}
