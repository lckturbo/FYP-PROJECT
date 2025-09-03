using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicFSM : EnemyBase
{
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

    protected override void Attack() { }
}
