using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniBoss : EnemyBase
{
    protected override void StateMachine()
    {
        switch (enemyStates)
        {
            case EnemyStates.Idle:
                Idle();
                break;
        }
    }
}
