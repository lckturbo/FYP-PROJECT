using System.Collections;
using System.Security.Cryptography;
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

    protected GameObject player;
    protected EnemyStates _states;
    [SerializeField] private Transform[] _enemyWP;
    [SerializeField] private Rigidbody2D _rb2D;
    private int _currWPIndex;
    [SerializeField] private float _speed;

    [Header("Health")]
    private float _currHealth;
    [SerializeField] private float _maxHealth;

    [Header("Idle")]
    [SerializeField] private float _idleTimer;
    private float _currIdleTimer;

    [Header("Attack")]
    [SerializeField] protected float _atkRange;
    [SerializeField] protected float _atkDmg;
    protected float _atkCD;

    [Header("Chase")]
    [SerializeField] private float _chaseRange;

    [Header("Investigate")]
    [SerializeField] private float _investigateTimer;
    private float _currInvTimer;

    private void Start()
    {
        if (player == null) 
            player = GameObject.FindWithTag("Player");

        _currHealth = _maxHealth;
        _states = EnemyStates.Idle;
        
        // timers
        _currIdleTimer = _idleTimer;
        _currInvTimer = _investigateTimer;
        _currWPIndex = 0;
    }

    private void Update()
    {
        StateMachine();

        // for testing
        if (Input.GetKeyDown(KeyCode.L))
            TakeDamage(50);
    }

    protected virtual void Idle()
    {
        Debug.Log("(Enemy) Idle State");
        if (_currIdleTimer <= _idleTimer)
        {
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

        Debug.Log("(Enemy) Patrol State");
        Transform target = _enemyWP[_currWPIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, _speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, player.transform.position) < _chaseRange)
            _states = EnemyStates.Chase;

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            _currWPIndex++;
            _states = EnemyStates.Idle;

            if (_currWPIndex >= _enemyWP.Length)
                _currWPIndex = 0;
        }

    }
    protected abstract void Attack();
    protected virtual void TakeDamage(float amt)
    {
        if (_currHealth <= 0)
            _states = EnemyStates.Death;

        _currHealth -= amt;
    }
    protected virtual void Chase()
    {
        if (player == null) return;

        Debug.Log("(Enemy) Chase State");
        Vector3 playerPos = player.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, playerPos, _speed * Time.deltaTime);

        if(Vector3.Distance(transform.position, playerPos) < _atkRange)
            _states = EnemyStates.Attack;

        if(Vector3.Distance(transform.position, playerPos) > _chaseRange)
            _states = EnemyStates.Investigate;
    }
    protected virtual void Investigate()
    {
        Debug.Log("(Enemy) Investigate State");
        if(_currInvTimer <= _investigateTimer)
        {
            _currInvTimer -= Time.deltaTime;
            if(_currInvTimer <= 0)
            {
                _states = EnemyStates.Idle;
            }
        }
    }
    protected virtual void Death()
    {
        Debug.Log("(Enemy) Death State");
        // play animation
        Destroy(gameObject, 2f);
        // respawn new enemy
    }

    // mobs -> all fsm
    // mini-boss -> patrol(?), combat, investigate, chase
    protected abstract void StateMachine();
}
