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
    [SerializeField] protected EnemyStates _enemyStates;
    [SerializeField] protected EnemyStats _enemyStats;
    public EnemyStats GetEnemyStats() => _enemyStats;
    protected Transform _player;
    [SerializeField] protected Animator _animator;

    [Header("Enemy Components")]
    [SerializeField] protected NewHealth _health;
    [SerializeField] protected Seeker _seeker;
    [SerializeField] protected Rigidbody2D _rb;
    [SerializeField] protected AIPath _aiPath;
    private Path _path;
    private int _currWP;
    private bool _reachedPath;
    private float _currIdleTimer;
    private Waypoints _targetwp;

    [SerializeField] private float detectionDist;
    [SerializeField] private float sideOffSet; // left/right raycast
    [SerializeField] private float avoidanceStrength;

    //public event Action<GameObject, float> OnDeath;
    public event Action<GameObject, EnemyParty> OnAttackPlayer;
    protected bool inBattle;

    private void Awake()
    {
        if (!_enemyStats || !_aiPath || !_rb || !_seeker) return;
        _enemyStates = EnemyStates.Idle;
        _aiPath.maxSpeed = _enemyStats.Speed;
    }

    protected virtual void Start()
    {
        if (!_player) _player = GameObject.FindWithTag("Player").transform;
        if (!_animator) _animator = GetComponent<Animator>();
        if (!_health) _health = GetComponent<NewHealth>();
        _health.ApplyStats(_enemyStats);

        _currIdleTimer = _enemyStats.idleTimer;
    }
    private void Update()
    {
        StateMachine();

        if (Input.GetKeyDown(KeyCode.L))
            _health.TakeDamage(10, _enemyStats);
    }
    protected virtual void Idle()
    {
        if (_animator) _animator.Play("IdleBack");
        _aiPath.canMove = false;
        _rb.velocity = Vector2.zero;
        if (CanSeePlayer())
        {
            _enemyStates = EnemyStates.Chase;
            return;
        }

        if (!_targetwp || !_targetwp.isOccupied())
        {
            _targetwp = WayPointManager.instance.GetFreeWayPoint();
            if (_targetwp) _targetwp.SetOccupied(true);
        }

        _currIdleTimer -= Time.deltaTime;
        if (_currIdleTimer <= 0 && _targetwp)
        {
            _enemyStates = EnemyStates.Patrol;
            _currIdleTimer = _enemyStats.idleTimer;
        }
    }

    protected virtual void Patrol()
    {
        if (!_aiPath || !WayPointManager.instance || !_targetwp)
            return;

        Vector2 waypoint = _targetwp.transform.position;
        Vector2 avoidance = SteeringAvoidance() + Separation();
        Vector2 final = waypoint + avoidance;
        _aiPath.canMove = true;
        _aiPath.destination = final;

        Vector2 dir = ((Vector2)_aiPath.steeringTarget - (Vector2)transform.position).normalized;
        FaceDir(dir);

        if (_aiPath.reachedEndOfPath || _aiPath.remainingDistance < 0.5f)
        {
            _targetwp.SetOccupied(false);

            foreach (var neighbor in _targetwp.nearestWaypoints)
            {
                if (!neighbor.isOccupied())
                {
                    _targetwp = neighbor;
                    _targetwp.SetOccupied(true);
                    break;
                }
            }
            _enemyStates = EnemyStates.Idle;
        }
        else if (_aiPath.velocity.sqrMagnitude < 0.1f)
            _aiPath.SearchPath();

        if (CanSeePlayer()) _enemyStates = EnemyStates.Chase;
    }

    private Vector2 SteeringAvoidance()
    {
        if (_aiPath.velocity.sqrMagnitude <= 0.01f) return Vector2.zero;

        LayerMask enemyLayer = LayerMask.GetMask("Enemy");
        RaycastHit2D closestHit = new RaycastHit2D();
        float minF = float.MaxValue;

        // forward
        RaycastHit2D hit = Physics2D.Raycast(transform.position, _aiPath.desiredVelocity.normalized, detectionDist, enemyLayer);
        if (hit && hit.collider.gameObject != gameObject && hit.fraction < minF)
        {
            closestHit = hit;
            minF = hit.fraction;
        }

        // sides
        Vector2 perp = new Vector2(_aiPath.desiredVelocity.y, -_aiPath.desiredVelocity.x).normalized * sideOffSet;
        RaycastHit2D leftHit = Physics2D.Raycast((Vector2)transform.position - perp, _aiPath.desiredVelocity.normalized, detectionDist, enemyLayer);
        if (leftHit && leftHit.collider.gameObject != gameObject && leftHit.fraction < minF)
        {
            closestHit = leftHit;
            minF = leftHit.fraction;
        }
        RaycastHit2D rightHit = Physics2D.Raycast((Vector2)transform.position + perp, _aiPath.desiredVelocity.normalized, detectionDist, enemyLayer);
        if (rightHit && rightHit.collider.gameObject != gameObject && rightHit.fraction < minF)
        {
            closestHit = rightHit;
            minF = rightHit.fraction;
        }

        if (!closestHit.collider) return Vector2.zero;

        // steer left or right
        Vector2 toOther = (Vector2)closestHit.collider.transform.position - (Vector2)transform.position;
        float cross = _aiPath.desiredVelocity.x * -toOther.y + _aiPath.desiredVelocity.y * toOther.x;
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
        if (!_player) return;
        float dist = Vector2.Distance(_rb.position, _player.position);

        if (dist > _enemyStats.chaseRange)
        {
            _aiPath.destination = _rb.position;
            _enemyStates = EnemyStates.Idle;
            return;
        }

        _aiPath.canMove = true;
        _aiPath.destination = _player.position;

        if (dist < _enemyStats.atkRange)
        {
            _aiPath.canMove = false;
            _enemyStates = EnemyStates.Attack;
        }
    }

    protected virtual void Attack()
    {
        //float dir = player.position.x - transform.position.x;
        //FaceDir(dir);
        //PlayerNearby();

        if (CanSeePlayer()) _enemyStates = EnemyStates.Chase;

        // TODO: play "hit" animation (don't need to deduct player's health -> transition to jasBattle scene
        if (!inBattle)
        {
            BattleSystem.instance.RegisterEnemy(this);
            OnAttackPlayer.Invoke(_player.gameObject, GetComponent<EnemyParty>());
            inBattle = true;

            //if (battleScene && normalScene)
            //{
            //    normalScene.SetActive(false);
            //    battleScene.SetActive(true);
            //}
            // CHANGE SCENE
            GameManager.instance.ChangeScene("jasBattle");
        }
        _enemyStates = EnemyStates.Idle;
    }

    protected abstract void BattleAttack();

    protected virtual bool CanSeePlayer()
    {
        if (!_player) return false;

        float dist = Vector2.Distance(transform.position, _player.position);

        if (dist > _enemyStats.chaseRange)
            return false;

        Vector2 distToPlayer = (_player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, distToPlayer, _enemyStats.chaseRange, LayerMask.GetMask("Player"));

        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    // NORMAL -> ALL FSM
    // BATTLE -> IDLE(waiting turn) -> ATTACK
    protected virtual void StateMachine()
    {
        switch (_enemyStates)
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
    //    //OnDeath?.Invoke(this.gameObject, _deathTime);
    //    //Destroy(gameObject, _deathTime);
    //}

    protected void FaceDir(Vector2 dir)
    {
        if (!_player || !_animator) return;

        Vector2 moveDir = Vector2.zero;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            SpriteRenderer _sprite = GetComponent<SpriteRenderer>();
            // move horizontal
            if (dir.x > 0)
            {
                moveDir.x = 1;
                _animator.Play("WalkRight");
                _sprite.flipX = false;
            }
            else
            {
                moveDir.x = -1;
                _animator.Play("WalkRight");
                _sprite.flipX = true;
            }
        }
        else
        {
            // move vertical
            if (dir.y > 0)
            {
                moveDir.y = 1;
                _animator.Play("WalkFront");
            }
            else
            {
                moveDir.y = -1;
                _animator.Play("WalkBack");
            }
        }
    }
}
