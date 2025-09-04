using UnityEngine;

public class BasicEnemy : EnemyBase
{
    protected override void Attack()
    {
        Debug.Log("(Enemy) Attack State");

        PlayerNearby();
        // play animation
        // player take damage
        if (_currAtkTimer <= _atkCD)
        {
            _currAtkTimer -= Time.deltaTime;

            if (_currAtkTimer <= 0)
            {
                Health playerHealth = player.GetComponent<Health>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(_atkDmg);
                _currAtkTimer = _atkCD;
            }
        }
    }

    protected override void StateMachine()
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
                Investigate();
                break;
            case EnemyStates.Chase:
                Chase();
                break;
            case EnemyStates.Death:
                Death();
                break;
        }
    }
}
