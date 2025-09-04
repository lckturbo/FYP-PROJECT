using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    public enum EnemyStates
    {
        Idle,
        Patrol,
        Attack,
        Investigate,
        Chase,
        Death
    }

    [Header("Enemy Stats")]
    [SerializeField] protected EnemyStats _enemyStats;

    protected Transform player;
    protected EnemyStates _states;
    [SerializeField] private Transform[] _enemyWP;
    private int _currWPIndex;
    private float _speed;

    [Header("Health")]
    private float _currHealth;
    private float _maxHealth;

    [Header("Idle")]
    private float _idleTimer;
    private float _currIdleTimer;

    [Header("Attack")]
    protected int _atkRange;
    protected int _atkDmg;
    protected float _atkCD;
    protected float _currAtkTimer;

    // CHASE //
    private float _chaseRange;

    // INVESTIGATE //
    private float _investigateTimer;
    private float _currInvTimer;
    // FOR TANKS //
    private float _dmgReduction;

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
    }

    protected virtual void Start()
    {
        if (_enemyStats == null) return;
        if (player == null) player = GameObject.FindWithTag("Player").transform;
        Initialize(_enemyStats);

        _currHealth = _maxHealth;
        _states = EnemyStates.Idle;

        // timers
        _currIdleTimer = _idleTimer;
        _currInvTimer = _investigateTimer;
        _currAtkTimer = _atkCD;
        _currWPIndex = 0;
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
        MsgLog("Idle State");
        if (_currIdleTimer <= _idleTimer)
        {
            //LookForPlayer();
            if (PlayerNearby()) return;
            _currIdleTimer -= Time.deltaTime;

            if (_currIdleTimer <= 0)
            {
                _states = EnemyStates.Patrol;
                _currIdleTimer = _idleTimer;
            }
        }
    }
    protected virtual void Patrol()
    {
        if (_enemyWP.Length == 0 || _enemyWP == null) return;

        MsgLog("Patrol State");

        Transform target = _enemyWP[_currWPIndex];
        float dir = target.position.x - transform.position.x;
        FaceDir(dir);

        transform.position = Vector3.MoveTowards(transform.position, target.position, _speed * Time.deltaTime);

        if (PlayerNearby()) return;

        if (Vector3.Distance(transform.position, target.position) < 0.01f)
        {
            _currWPIndex++;
            _states = EnemyStates.Idle;

            if (_currWPIndex >= _enemyWP.Length)
                _currWPIndex = 0;
        }

    }
    protected virtual void Attack()
    {
        float dir = player.position.x - transform.position.x;
        FaceDir(dir);
        // transition to battle scene once successfully hit player
        // notify battlesys enemyturn is over
    }

    // PLAYER -> CALL TO KILL ENEMIES
    public virtual void TakeDamage(float amt)
    {
        if (_currHealth <= 0)
            _states = EnemyStates.Death;

        switch (_enemyStats.type)
        {
            case EnemyStats.EnemyTypes.Basic:
                _currHealth -= amt;
                break;
            case EnemyStats.EnemyTypes.Tank:
                _currHealth -= Mathf.RoundToInt(amt * (1f - _dmgReduction));
                break;
            case EnemyStats.EnemyTypes.MiniBoss:
                break;
        }

        MsgLog(_enemyStats.type + " HP: " + _currHealth + "/" + _maxHealth);
    }
    protected virtual void Chase()
    {
        if (player == null) return;

        PlayerNearby();
        MsgLog("Chase State");

        float dir = player.position.x - transform.position.x;
        FaceDir(dir);

        Vector3 playerPos = player.position;
        playerPos.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, playerPos, _speed * Time.deltaTime);
    }
    protected virtual void Investigate()
    {
        MsgLog("Investigate State");
        if (_currInvTimer <= _investigateTimer)
        {
            //LookForPlayer();
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
        MsgLog("Death State");
        // play animation
        Destroy(gameObject, 2f);
        // respawn new enemy
    }
    protected virtual bool PlayerNearby()
    {
        if (player == null) return false;
        //Debug.Log("(Enemy) Player is nearby");
        Vector2 dirToPlayer = (player.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, player.position);
        LayerMask playerLayer = LayerMask.GetMask("Player");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, _chaseRange, playerLayer);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            MsgLog("Player found");
            if (dist <= _chaseRange)
            {
                if (dist <= _atkRange)
                    _states = EnemyStates.Attack;
                else
                    _states = EnemyStates.Chase;
            }
            else
            {
                _states = EnemyStates.Investigate;
            }
            return true;
        }
        return false;
    }

    protected void FaceDir(float dir)
    {
        if (player == null) return;

        SpriteRenderer enemySprite = GetComponent<SpriteRenderer>();
        Vector2 scale = transform.localScale;
        if (dir > 0)
        {
            enemySprite.flipX = true;
            scale.x = Mathf.Abs(scale.x);
            MsgLog("dir > 0: " + transform.localScale.x);
        }
        else if (dir < 0)
        {
            enemySprite.flipX = false;
            scale.x = -Mathf.Abs(scale.x);
            MsgLog("dir < 0: " + transform.localScale.x);
        }

        transform.localScale = scale;
    }

    // mobs -> all fsm
    // mini-boss -> patrol(?), combat, investigate, chase
    protected abstract void StateMachine();

    private void MsgLog(string msg)
    {
        if (msg != null)
            Debug.Log("(Enemy) " + msg);
    }
}
