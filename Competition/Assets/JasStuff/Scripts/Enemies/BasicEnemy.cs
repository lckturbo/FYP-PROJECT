using UnityEngine;

public class BasicEnemy : EnemyBase
{
    protected override void Attack()
    {
        if (isAttacking) return;

        if (player)
        {
            Vector2 lookDir = (player.position - transform.position).normalized;
            anim.SetFloat("moveX", lookDir.x);
            anim.SetFloat("moveY", lookDir.y);
            lastMoveDir = lookDir;
        }

        aiPath.canMove = false;
        isAttacking = true;

        anim.SetTrigger("attack");
    }

}
