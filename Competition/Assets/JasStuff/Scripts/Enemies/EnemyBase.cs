using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    public enum EnemyStates
    {
        Idle,
        Patrol,
        Alert,
        Attack,
        Chase
    }
    public enum PatrolDirection
    {
        Forward,
        Backward
    }

    [Header("Enemy States")]
    [SerializeField] protected EnemyStates enemyStates;
    [SerializeField] protected EnemyStats enemyStats;
    public EnemyStats GetEnemyStats() => enemyStats;
    protected Transform player;

    [Header("Enemy Components")]
    [SerializeField] protected Animator anim;
    [SerializeField] protected NewHealth health;
    [SerializeField] protected Seeker seeker;
    [SerializeField] protected Rigidbody2D rb2d;
    [SerializeField] protected AIPath aiPath;

    [Header("Enemy Settings")]
    [SerializeField] protected int areaID;
    protected float idleTimer = 2;

    [Header("Battle UI")]
    public Sprite enemyPortrait;

    [Header("Hitboxes")]
    [SerializeField] private Transform attackHitbox;
    [SerializeField] private Vector2 upOffset = new Vector2(0f, 0.5f);
    [SerializeField] private Vector2 downOffset = new Vector2(0f, -0.5f);
    [SerializeField] private Vector2 leftOffset = new Vector2(-0.5f, 0f);
    [SerializeField] private Vector2 rightOffset = new Vector2(0.5f, 0f);
    [SerializeField] private Collider2D hitboxCollider;

    [Header("Waypoints Settings")]
    //protected WayPointArea patrolArea;
    protected int currId = 0;
    protected Vector2 currTarget;
    protected List<Transform> waypoints;

    [Header("Patrol Settings")]
    protected int dirStep = 1;

    public event Action<EnemyParty> OnAttackPlayer;
    protected Vector2 lastMoveDir = Vector2.down;
    protected bool fullyBlockingAlert = false;

    protected bool isAttacking;
    protected bool alertCoroutineStarted = false;
    public bool IsAttacking() => isAttacking;
    protected float attackCooldownTimer = 0f;

    protected bool isAlerting = false;
    protected bool returningFromChase = false;

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeedMultiplier = 1.5f;

    private float chaseMemoryTimer = 0f;
    [SerializeField] private float chaseMemoryDuration = 1.25f;

    private float chaseTime = 0f;


    public void EnableHitBox() => hitboxCollider.enabled = true;
    public void DisableHitBox() => hitboxCollider.enabled = false;

    private void OnEnable() { PlayerSpawner.OnPlayerSpawned += HandlePlayerSpawned; }
    private void OnDisable() { PlayerSpawner.OnPlayerSpawned -= HandlePlayerSpawned; }
    private void HandlePlayerSpawned(Transform playerTransform)
    {
        player = playerTransform;
    }

    private void Awake()
    {
        if (!enemyStats || !aiPath || !rb2d || !seeker) return;

        enemyStates = EnemyStates.Idle;

        aiPath.maxSpeed = enemyStats.Speed;

        aiPath.maxAcceleration = 999f;
        aiPath.slowWhenNotFacingTarget = false;
        aiPath.enableRotation = false; 

        hitboxCollider.enabled = false;
    }


    protected virtual void Start()
    {
        if (!anim) anim = GetComponent<Animator>();
        if (!health) health = GetComponent<NewHealth>();
        health.ApplyStats(enemyStats);

        if (attackHitbox)
        {
            EnemyHitbox enemyHitBox = attackHitbox.GetComponent<EnemyHitbox>();
            if (enemyHitBox)
                enemyHitBox.Init(this);
        }


    }

    private void Update()
    {
        StateMachine();

        if (attackCooldownTimer > 0f) attackCooldownTimer -= Time.deltaTime;
    }

    // normal/boss: all fsm [boss tbc, might change fsm]
    // battle [turnbased]: idle -> attack
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
            case EnemyStates.Alert:
                Alert();
                break;
            case EnemyStates.Chase:
                Chase();
                break;
            case EnemyStates.Attack:
                Attack(); // trigger battle scene
                break;
        }
    }

    // idle -> patrol -> chase -> attack -> chase -> idle [repeat]

    // ---- IDLE ---- //
    protected virtual void Idle()
    {
        AudioManager.instance.StopLoopingSFX(gameObject);


        rb2d.velocity = Vector2.zero;
        anim.SetFloat("speed", 0f);

        chaseTime = 0f;
        chaseMemoryTimer = 0f;
    }

    // ---- PATROL ---- //
    protected virtual void Patrol()
    {
        if (!aiPath) return;

        AudioManager.instance.PlayLoopingSFXAtObject("boots-on-pavement-66709", gameObject, 1);

        if (returningFromChase)
        {
            currId = GetNearestWaypointIndex();
            currTarget = waypoints[currId].position;
            returningFromChase = false;
        }

        aiPath.canMove = true;
        aiPath.destination = currTarget;
        UpdateAnim();

        if (CanSeePlayer())
        {
            enemyStates = EnemyStates.Alert;
            return;
        }

        if (Vector2.Distance(transform.position, currTarget) < 0.2f)
        {
            currId += dirStep;

            if (currId >= waypoints.Count)
                currId = 0;
            else if (currId < 0)
                currId = waypoints.Count - 1;

            currTarget = waypoints[currId].position;
            enemyStates = EnemyStates.Idle;
        }
    }

    // ---- ALERT ---- //
    protected virtual void Alert()
    {
        AudioManager.instance.StopLoopingSFX(gameObject);

        if (!alertCoroutineStarted)
        {
            alertCoroutineStarted = true;

            aiPath.isStopped = true;
            aiPath.canMove = false;
            rb2d.velocity = Vector2.zero;

            anim.SetFloat("speed", 0f);
            anim.Update(0f);

            anim.SetTrigger("alert");

            StartCoroutine(AlertDelay());
        }


        if (player)
        {
            Vector2 lookDir = (player.position - transform.position).normalized;
            anim.SetFloat("moveX", lookDir.x);
            anim.SetFloat("moveY", lookDir.y);
            lastMoveDir = lookDir;
        }
    }




    private IEnumerator AlertDelay()
    {
        yield return null;

        while (!anim.GetCurrentAnimatorStateInfo(0).IsName("alert"))
            yield return null;

        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        float alertLength = info.length;

        yield return new WaitForSeconds(alertLength);

        alertCoroutineStarted = false;

        aiPath.isStopped = false;
        aiPath.canMove = true;
        chaseTime = 0f;
        chaseMemoryTimer = chaseMemoryDuration;

        returningFromChase = true;
        enemyStates = EnemyStates.Chase;
    }




    // ---- ATTACK ---- //
    protected virtual void Attack()
    {
        if (isAttacking) return;

        AudioManager.instance.StopLoopingSFX(gameObject);


        if (player)
        {
            Vector2 lookDir = (player.position - transform.position).normalized;
            anim.SetFloat("moveX", lookDir.x);
            anim.SetFloat("moveY", lookDir.y);
            lastMoveDir = lookDir;
        }

        aiPath.canMove = false;
        isAttacking = true;

        anim.SetTrigger("attack1");
    }

    protected void UpdateHitboxDirection()
    {
        if (!attackHitbox) return;

        Vector2 offset = Vector2.zero;

        if (Mathf.Abs(lastMoveDir.x) > Mathf.Abs(lastMoveDir.y))
            offset = lastMoveDir.x > 0 ? rightOffset : leftOffset;
        else
            offset = lastMoveDir.y > 0 ? upOffset : downOffset;

        attackHitbox.localPosition = offset;
    }

    public void EndAttack()
    {
        isAttacking = false;
        attackCooldownTimer = 1f;

        aiPath.canMove = true;
        enemyStates = EnemyStates.Chase;
    }

    public virtual void TriggerAttack()
    {
        BattleManager.instance.RegisterEnemy(this);
        OnAttackPlayer.Invoke(GetComponent<EnemyParty>());
        BattleManager.instance.SetBattleMode(true);
        Debug.Log("battle");
    }

    // ---- CHASE ---- //
    protected virtual void Chase()
    {
        if (!player) return;

        AudioManager.instance.PlayLoopingSFXAtObject("boots-on-pavement-66709", gameObject, 1);

        float dist = Vector2.Distance(rb2d.position, player.position);
        if (!CanSeePlayer())
        {
            chaseMemoryTimer -= Time.deltaTime;
            if (chaseMemoryTimer <= 0f)
            {
                aiPath.destination = rb2d.position;
                aiPath.maxSpeed = enemyStats.Speed;

                chaseTime = 0f;
                returningFromChase = true;
                enemyStates = EnemyStates.Idle;
                return;
            }
        }
        else
        {
            chaseMemoryTimer = chaseMemoryDuration;
        }
        aiPath.canMove = true;
        aiPath.isStopped = false;

        Vector2 playerVel = player.GetComponent<Rigidbody2D>().velocity;
        Vector2 predictedPos = (Vector2)player.position + playerVel * 0.6f;
        aiPath.destination = predictedPos;

        chaseTime += Time.deltaTime;
        float ramp = 1f + chaseTime * 0.15f;
        aiPath.maxSpeed = enemyStats.Speed * chaseSpeedMultiplier * ramp;

        if (dist < enemyStats.atkRange + 0.5f && !isAttacking && attackCooldownTimer <= 0f)
            enemyStates = EnemyStates.Attack;

        UpdateAnim();
    }

    protected virtual bool CanSeePlayer()
    {
        if (!player) return false;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > enemyStats.chaseRange) return false;

        Vector2 rayOrigin = rb2d.position;
        Vector2 dirToPlayer = ((Vector2)player.position - rayOrigin).normalized;

        int mask = LayerMask.GetMask("Player", "Obstacle", "Beam");
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, dirToPlayer, enemyStats.chaseRange, mask);
        if (!hit.collider) return false;

        return hit.collider.CompareTag("Player");
    }

    protected void UpdateAnim()
    {
        if (!anim) return;

        float speed = aiPath.canMove ? aiPath.velocity.magnitude : 0f;

        if (aiPath.reachedDestination || !aiPath.canMove || speed < 0.1f)
            speed = 0f;

        anim.SetFloat("speed", speed);

        Vector2 dir;

        if (speed > 0.1f)
        {
            dir = aiPath.velocity.normalized;
            lastMoveDir = dir;
        }
        else if (enemyStates == EnemyStates.Chase && player)
        {
            dir = (player.position - transform.position).normalized;
            lastMoveDir = dir;
        }
        else dir = lastMoveDir;

        anim.SetFloat("moveX", dir.x);
        anim.SetFloat("moveY", dir.y);

        UpdateHitboxDirection();
    }

    protected int GetNearestWaypointIndex()
    {
        int nearest = 0;
        float minDist = float.MaxValue;

        for (int i = 0; i < waypoints.Count; i++)
        {
            float d = Vector2.Distance(transform.position, waypoints[i].position);
            if (d < minDist)
            {
                minDist = d;
                nearest = i;
            }
        }
        return nearest;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isAttacking)
        {
            if (enemyStates == EnemyStates.Idle || enemyStates == EnemyStates.Patrol)
            {
                if (CanSeePlayer())
                {
                    enemyStates = EnemyStates.Alert;
                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        if (!enemyStats) return;

        Gizmos.color = new Color(1f, 1f, 0f, 0.3f); 
        Gizmos.DrawWireSphere(transform.position, enemyStats.chaseRange);
    }
}
