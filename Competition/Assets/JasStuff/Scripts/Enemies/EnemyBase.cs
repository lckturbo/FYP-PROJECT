using Pathfinding;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    public enum EnemyStates
    {
        Idle,
        Patrol,
        Attack,
        BattleAttack,
        Chase,
        Death
    }

    [Header("Enemy States")]
    [SerializeField] protected EnemyStates _enemyStates;
    [SerializeField] protected EnemyStats _enemyStats;
    protected Transform _player;

    [Header("Enemy Components")]
    [SerializeField] protected Seeker _seeker;
    [SerializeField] protected Rigidbody2D _rb;
    [SerializeField] protected AIPath _aiPath;
    [SerializeField] protected float _speed;
    private Path _path;
    private int _currWP;
    private bool _reachedPath;

    //[Header("Health")]
    //private int _currHealth;
    //public int GetCurrHealth() => _currHealth;
    //private int _maxHealth;
    //public int GetMaxHealth() => _maxHealth;

    //[Header("Idle")]
    private float _idleTimer;
    private float _currIdleTimer;
    private float _chaseRange;
    protected float _atkRange;
    //protected int _atkDmg;
    //protected float _AOERadius;

    //// INVESTIGATE //
    //private float _investigateTimer;
    //private float _currInvTimer;
    //// FOR TANKS //
    //private float _dmgReduction;
    //// FOR DEATH //
    //[SerializeField] private float _deathTime;
    //public float GetDeathTime() => _deathTime;

    //public event Action<GameObject, float> OnDeath;
    //public event Action<GameObject, EnemyBase> OnAttackPlayer;
    //protected bool inBattle;

    //[Header("FOR TESTING ONLY")]
    //[SerializeField] private GameObject normalScene;
    //[SerializeField] private GameObject battleScene;

    protected void Initialize(EnemyStats stats)
    {
        _speed = stats.speed;
        //_maxHealth = stats.maxHealth;
        _atkRange = stats.atkRange;
        //_atkDmg = stats.atkDmg;
        //_atkCD = stats.atkCD;
        _idleTimer = stats.idleTimer;
        _chaseRange = stats.chaseRange;
        //_investigateTimer = stats.investigateTimer;
        //_dmgReduction = stats.dmgReduction;
        //_AOERadius = stats.AOERadius;
    }
    private void Awake()
    {
        if (!_enemyStats || !_aiPath || !_rb || !_seeker) return;
        Initialize(_enemyStats);
        _enemyStates = EnemyStates.Idle;
        _aiPath.maxSpeed = _speed;
    }

    protected virtual void Start()
    {
        if (!_player) _player = GameObject.FindWithTag("Player").transform;

        _currIdleTimer = _idleTimer;
    }
    private void Update()
    {
        StateMachine();
    }
    protected virtual void Idle()
    {
        _rb.velocity = Vector2.zero;
        //_path = null;

        if (CanSeePlayer()) _enemyStates = EnemyStates.Chase;

        _currIdleTimer -= Time.deltaTime;
        if (_currIdleTimer <= 0)
        {
            _enemyStates = EnemyStates.Patrol;
            StartPath();
            _currIdleTimer = _idleTimer;
        }
    }

    protected virtual void Patrol()
    {
        if (_path == null || _reachedPath || _path.vectorPath.Count == 0) 
            return;
        Vector2 dir = ((Vector2)_path.vectorPath[0] - _rb.position).normalized;
        _rb.velocity = dir * _speed;

        //PlayerNearby();
        if (CanSeePlayer()) _enemyStates = EnemyStates.Chase;
        if (Vector2.Distance(_rb.position, (Vector2)_path.vectorPath[_path.vectorPath.Count - 1]) < 0.5f)
        {
            _currWP = (_currWP + 1) % WayPointManager.instance.GetTotalWayPoints();
            _enemyStates = EnemyStates.Idle;
        }
    }

    protected virtual void Chase()
    {
        if (!_player) return;

        float dist = Vector2.Distance(_rb.position, _player.position);

        if (dist > _chaseRange || !CanSeePlayer())
        {
            _enemyStates = EnemyStates.Idle;
            return;
        }

        _seeker.StartPath(_rb.position, _player.position, OnPathComplete);

        if (_path != null && _path.vectorPath.Count > 0)
        {
            Vector2 target = (Vector2)_path.vectorPath[0];
            Vector2 dir = (target - _rb.position).normalized;
            //_rb.velocity = dir * _speed;
            if (Vector2.Distance(_rb.position, target) < 0.2f)
                _path.vectorPath.RemoveAt(0);
        }
    }

    private void StartPath()
    {
        if (!WayPointManager.instance) return;

        Vector3 target = WayPointManager.instance.GetWayPoint(_currWP);
        _seeker.StartPath(_rb.position, target, OnPathComplete);
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            _path = p;
        }
    }

    protected virtual void Attack()
    {
        //float dir = player.position.x - transform.position.x;
        //FaceDir(dir);
        //PlayerNearby();

        // TODO: TRANSITION TO BATTLE SCENE ONCE !! SUCCESSFULLY HIT PLAYER !!
        //if (!inBattle)
        //{
        //    BattleSystem.instance.RegisterEnemy(this);
        //    OnAttackPlayer.Invoke(player.gameObject, this);
        //    inBattle = true;

        //    if (battleScene && normalScene)
        //    {
        //        normalScene.SetActive(false);
        //        battleScene.SetActive(true);
        //    }
        //    // CHANGE SCENE
        //}
        _enemyStates = EnemyStates.Idle;
    }

    protected abstract void BattleAttack();

    //protected virtual bool PlayerNearby()
    //{
    //    if (!_player) return false;

    //    Vector2 dirTo_player = (_player.position - transform.position).normalized;
    //    float dist = Vector3.Distance(transform.position, _player.position);
    //    LayerMask _playerLayer = LayerMask.GetMask("Player");
    //    RaycastHit2D hit = Physics2D.Raycast(transform.position, dirTo_player, _chaseRange, _playerLayer);

    //    if (hit.collider != null && hit.collider.CompareTag("Player"))
    //    {
    //        if (dist < _chaseRange)
    //        {
    //            Debug.Log("[Enemy] Chasing");
    //            if (dist < _atkRange)
    //                _enemyStates = EnemyStates.Attack;
    //            else
    //                _enemyStates = EnemyStates.Chase;
    //        }
    //        return true;
    //    }

    //    _enemyStates = EnemyStates.Idle;
    //    return false;
    //}

    protected virtual bool CanSeePlayer()
    {
        if (!_player) return false;

        float dist = Vector2.Distance(transform.position, _player.position);
        if (dist > _chaseRange) return false;

        Vector2 dirToPlayer = (_player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, _chaseRange);

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
            case EnemyStates.Death:
                //Death();
                break;
        }
    }

    //protected virtual void Start()
    //{
    //    if (!player) player = GameObject.FindWithTag("Player").transform;

    //    // timers
    //    _currIdleTimer = _idleTimer;
    //    _currInvTimer = _investigateTimer;
    //    _currAtkTimer = _atkCD;

    //    if (!inBattle)
    //    {
    //        Waypoints[] allWayPoints = FindObjectsOfType<Waypoints>();
    //        if (allWayPoints.Length > 0)
    //            _currWP = allWayPoints[UnityEngine.Random.Range(0, allWayPoints.Length)];
    //        //_currWPIndex = 0;
    //        //SetWP("EnemyWP");
    //    }
    //}

    //private void Update()
    //{
    //    StateMachine();

    //    // for testing
    //    if (Input.GetKeyDown(KeyCode.L))
    //        TakeDamage(10);
    //}

    //private void OnDrawGizmosSelected()
    //{
    //    if (_currWP)
    //    {
    //        Gizmos.color = Color.yellow;
    //        Gizmos.DrawLine(transform.position, _currWP.transform.position);
    //    }
    //}

    //// PLAYER -> CALL TO KILL ENEMIES
    //public virtual void TakeDamage(int amt)
    //{
    //    switch (_enemyStats.type)
    //    {
    //        case EnemyStats.EnemyTypes.Basic:
    //            _currHealth -= amt;
    //            break;
    //        default:
    //            _currHealth -= Mathf.RoundToInt(amt * (1f - _dmgReduction)); // tank and boss
    //            break;
    //    }

    //    MsgLog(_enemyStats.type + " HP: " + _currHealth + "/" + _maxHealth);

    //    if (_currHealth <= 0)
    //    {
    //        _currHealth = Mathf.Max(_currHealth, 0);
    //        _states = EnemyStates.Death;
    //    }
    //}
    //protected virtual void Chase()
    //{
    //    if (player == null) return;

    //    PlayerNearby();

    //    Vector2 dirToTarget = (player.position - transform.position).normalized;
    //    FaceDir(dirToTarget);

    //    transform.position = Vector3.MoveTowards(transform.position, player.position, _speed * Time.deltaTime);
    //}
    //protected virtual void Investigate()
    //{
    //    if (_currInvTimer <= _investigateTimer)
    //    {
    //        if (PlayerNearby()) return;
    //        _currInvTimer -= Time.deltaTime;
    //        if (_currInvTimer <= 0)
    //        {
    //            _states = EnemyStates.Idle;
    //            _currInvTimer = _investigateTimer;
    //        }
    //    }
    //}
    //protected virtual void Death()
    //{
    //    // TODO: ANIMATION
    //    OnDeath?.Invoke(this.gameObject, _deathTime);
    //    Destroy(gameObject, _deathTime);
    //}

    //protected void FaceDir(Vector2 dir)
    //{
    //    if (!player || !_animator) return;

    //    Vector2 moveDir = Vector2.zero;

    //    if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
    //    {
    //        SpriteRenderer _sprite = GetComponent<SpriteRenderer>();
    //        // move horizontal
    //        if (dir.x > 0)
    //        {
    //            moveDir.x = 1;
    //            _animator.Play("WalkRight");
    //            _sprite.flipX = false;
    //        }
    //        else
    //        {
    //            moveDir.x = -1;
    //            _animator.Play("WalkRight");
    //            _sprite.flipX = true;
    //        }
    //    }
    //    else
    //    {
    //        // move vertical
    //        if (dir.y > 0)
    //        {
    //            moveDir.y = 1;
    //            _animator.Play("WalkFront");
    //        }
    //        else
    //        {
    //            moveDir.y = -1;
    //            _animator.Play("WalkBack");
    //        }
    //    }
    //}

    //protected void MsgLog(string msg)
    //{
    //    Debug.Log("[Enemy] " + msg);
    //}
}
