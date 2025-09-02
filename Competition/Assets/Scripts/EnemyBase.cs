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
    protected EnemyStates _states;
    [SerializeField] private Transform[] _enemyWP;
    private int _currWPindex;
    [SerializeField] private float _speed;

    [Header("Health")]
    private float _currHealth;
    [SerializeField] private float _maxHealth;

    [Header("Idle")]
    [SerializeField] private float _idleTimer;
    private float _currTimer;

    [Header("Attack")]
    [SerializeField] private float _atkRange;
    [SerializeField] private float _atkDmg;
    private float _atkCD;

    [Header("Chase")]
    [SerializeField] private float _chaseRange;

    private void Start()
    {
        _currHealth = _maxHealth;
        _states = EnemyStates.Idle;
        _currTimer = _idleTimer;
        _currWPindex = 0;
    }

    private void Update()
    {
        StateMachine();
    }

    protected virtual void Idle()
    {
        if (_currTimer <= _idleTimer)
        {
            _currTimer -= Time.deltaTime;

            if (_currTimer <= 0)
            {
                _states = EnemyStates.Patrol;
                Debug.Log("currStates:" + _states);
                _currTimer = _idleTimer;
            }
        }
    }
    protected virtual void Patrol()
    {
        if (_enemyWP.Length == 0 || _enemyWP == null) return;
        Transform target = _enemyWP[_currWPindex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, _speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            _currWPindex++;
            _states = EnemyStates.Idle;

            if (_currWPindex >= _enemyWP.Length)
                _currWPindex = 0;
        }

    }
    protected abstract void Attack();
    protected virtual void TakeDamage()
    {
        if (_currHealth <= 0)
            _states = EnemyStates.Death;
    }
    protected virtual void Chase()
    {

    }
    protected virtual void Death()
    {
        // play animation
        // respawn new enemy
    }
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
            case EnemyStates.Attack:
                Attack();
                break;
            case EnemyStates.Investigate:
                break;
            case EnemyStates.Chase:
                break;
            case EnemyStates.Death:
                Death();
                break;
        }
    }
}
