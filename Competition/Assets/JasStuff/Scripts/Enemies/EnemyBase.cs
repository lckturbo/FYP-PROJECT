using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    public enum EnemyStates
    {
        Idle,
        Patrol,
        Attack,
        BattleAttack,
        Investigate,
        Chase,
        Death
    }

    [Header("Enemy Stats")]
    [SerializeField] protected EnemyStats _enemyStats;
    [Header("Animator")]
    [SerializeField] protected Animator _animator;
    protected Transform player;
    [Header("Enemy States")]
    public EnemyStates _states;

    // WAYPOINTS //
    private Waypoints _currWP;
    //[SerializeField] private Transform[] _enemyWP;
    //private int _currWPIndex;
    private float _speed;

    [Header("Health")]
    private int _currHealth;
    public int GetCurrHealth() => _currHealth;
    private int _maxHealth;
    public int GetMaxHealth() => _maxHealth;

    [Header("Idle")]
    private float _idleTimer;
    private float _currIdleTimer;

    [Header("Attack")]
    protected float _atkRange;
    protected int _atkDmg;
    protected float _atkCD;
    protected float _currAtkTimer;
    protected float _AOERadius;

    // CHASE //
    private float _chaseRange;
    // INVESTIGATE //
    private float _investigateTimer;
    private float _currInvTimer;
    // FOR TANKS //
    private float _dmgReduction;
    // FOR DEATH //
    [SerializeField] private float _deathTime;
    public float GetDeathTime() => _deathTime;

    public event Action<GameObject, float> OnDeath;
    public event Action<GameObject, EnemyBase> OnAttackPlayer;
    protected bool inBattle;

    [Header("FOR TESTING ONLY")]
    [SerializeField] private GameObject normalScene;
    [SerializeField] private GameObject battleScene;

    protected void Initialize(EnemyStats stats)
    {
        _speed = stats.speed;
        _maxHealth = stats.maxHealth;
        _atkRange = stats.atkRange;
        _atkDmg = stats.atkDmg;
        _atkCD = stats.atkCD;
        _idleTimer = stats.idleTimer;
        _chaseRange = stats.chaseRange;
        _investigateTimer = stats.investigateTimer;
        _dmgReduction = stats.dmgReduction;
        _AOERadius = stats.AOERadius;
    }

    private void Awake()
    {
        if (!_enemyStats) return;
        Initialize(_enemyStats);
        _currHealth = _maxHealth;
        _states = EnemyStates.Idle;
    }
    protected virtual void Start()
    {
        if (!player) player = GameObject.FindWithTag("Player").transform;

        // timers
        _currIdleTimer = _idleTimer;
        _currInvTimer = _investigateTimer;
        _currAtkTimer = _atkCD;

        if (!inBattle)
        {
            Waypoints[] allWayPoints = FindObjectsOfType<Waypoints>();
            if (allWayPoints.Length > 0)
                _currWP = allWayPoints[UnityEngine.Random.Range(0, allWayPoints.Length)];
            //_currWPIndex = 0;
            //SetWP("EnemyWP");
        }
    }

    private void Update()
    {
        StateMachine();

        // for testing
        if (Input.GetKeyDown(KeyCode.L))
            TakeDamage(10);
    }

    protected virtual void Idle()
    {
        if (_animator) _animator.Play("IdleBack");

        if (_currIdleTimer <= _idleTimer)
        {
            if (PlayerNearby()) return;
            _currIdleTimer -= Time.deltaTime;
            if (_currIdleTimer <= 0)
            {
                if (!inBattle) // checks if its in battle
                    _states = EnemyStates.Patrol;
                _currIdleTimer = _idleTimer;
            }
        }
    }
    protected virtual void Patrol()
    {
        if (!_currWP || _currWP.nearestWaypoints.Count == 0) return;

        Transform target = _currWP.transform;
        Vector2 dirToTarget = (target.position - transform.position).normalized;
        float checkDist = 1.0f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToTarget, checkDist, LayerMask.GetMask("Enemy"));

        if(hit.collider && hit.collider.gameObject != gameObject)
        {
            Debug.Log($"{name} sees enemy in the way: {hit.collider.name}");
            return;
        }

        FaceDir(dirToTarget);

        transform.position = Vector3.MoveTowards(transform.position, target.position, _speed * Time.deltaTime);

        if (PlayerNearby()) return;

        if (Vector2.Distance(transform.position, target.position) < 0.01f)
        {
            _currWP.SetOccupied(false);

            List<Waypoints> allWaypoints = _currWP.nearestWaypoints.FindAll(wp => !wp.isOccupied());
            if (allWaypoints.Count > 0)
            {
                _currWP = allWaypoints[UnityEngine.Random.Range(0, allWaypoints.Count)];
                _currWP.SetOccupied(true);
            }
            _states = EnemyStates.Idle;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_currWP)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, _currWP.transform.position);
        }
    }
    protected virtual void Attack()
    {
        //float dir = player.position.x - transform.position.x;
        //FaceDir(dir);
        //PlayerNearby();

        // TODO: TRANSITION TO BATTLE SCENE ONCE !! SUCCESSFULLY HIT PLAYER !!
        if (!inBattle)
        {
            BattleSystem.instance.RegisterEnemy(this);
            OnAttackPlayer.Invoke(player.gameObject, this);
            inBattle = true;

            if (battleScene && normalScene)
            {
                normalScene.SetActive(false);
                battleScene.SetActive(true);
            }
            // CHANGE SCENE
        }
        _states = EnemyStates.Idle;
    }

    protected abstract void BattleAttack();

    // PLAYER -> CALL TO KILL ENEMIES
    public virtual void TakeDamage(int amt)
    {
        switch (_enemyStats.type)
        {
            case EnemyStats.EnemyTypes.Basic:
                _currHealth -= amt;
                break;
            default:
                _currHealth -= Mathf.RoundToInt(amt * (1f - _dmgReduction)); // tank and boss
                break;
        }

        MsgLog(_enemyStats.type + " HP: " + _currHealth + "/" + _maxHealth);

        if (_currHealth <= 0)
        {
            _currHealth = Mathf.Max(_currHealth, 0);
            _states = EnemyStates.Death;
        }
    }
    protected virtual void Chase()
    {
        if (player == null) return;

        PlayerNearby();

        Vector2 dirToTarget = (player.position - transform.position).normalized;
        FaceDir(dirToTarget);

        transform.position = Vector3.MoveTowards(transform.position, player.position, _speed * Time.deltaTime);
    }
    protected virtual void Investigate()
    {
        if (_currInvTimer <= _investigateTimer)
        {
            if (PlayerNearby()) return;
            _currInvTimer -= Time.deltaTime;
            if (_currInvTimer <= 0)
            {
                _states = EnemyStates.Idle;
                _currInvTimer = _investigateTimer;
            }
        }
    }
    protected virtual void Death()
    {
        // TODO: ANIMATION
        OnDeath?.Invoke(this.gameObject, _deathTime);
        Destroy(gameObject, _deathTime);
    }
    protected virtual bool PlayerNearby()
    {
        if (!player) return false;

        Vector2 dirToPlayer = (player.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, player.position);
        LayerMask playerLayer = LayerMask.GetMask("Player");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, _chaseRange, playerLayer);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            if (dist < _chaseRange)
            {
                if (dist < _atkRange)
                    _states = EnemyStates.Attack;
                else
                    _states = EnemyStates.Chase;
            }
            else
                _states = EnemyStates.Idle;

            return true;
        }
        return false;
    }

    protected void FaceDir(Vector2 dir)
    {
        if (!player || !_animator) return;

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

    // NORMAL -> ALL FSM
    // BATTLE -> IDLE(waiting turn) -> ATTACK
    protected virtual void StateMachine()
    {
        switch (_states)
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
                BattleAttack();
                break;
            case EnemyStates.Death:
                Death();
                break;
        }
    }

    protected void MsgLog(string msg)
    {
        Debug.Log("[Enemy] " + msg);
    }
}
