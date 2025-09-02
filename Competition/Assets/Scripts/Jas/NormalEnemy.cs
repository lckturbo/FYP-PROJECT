using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalEnemy : EnemyBase
{
    protected override void Attack()
    {
        Debug.Log("(Enemy) Attack State");

        if (Vector3.Distance(transform.position, player.transform.position) > _atkRange)
            _states = EnemyStates.Chase;

        // play animation
        // player take damage
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
