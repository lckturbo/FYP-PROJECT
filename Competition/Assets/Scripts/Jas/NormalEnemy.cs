using UnityEngine;

public class NormalEnemy : BasicFSM
{
    protected override void Attack()
    {
        Debug.Log("(Enemy) Attack State");

        if (Vector3.Distance(transform.position, player.transform.position) > _atkRange)
            _states = EnemyStates.Chase;

        // play animation
        // player take damage
    }
}
