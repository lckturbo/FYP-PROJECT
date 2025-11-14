using UnityEngine;

public class MiniBoss : EnemyBase
{
    private float rotateTimer = 0f;
    private float rotateInterval = 1.5f; 
    private int directionIndex = 0;

    private Vector2[] idleDirections =
    {
        Vector2.up,
        Vector2.right,
        Vector2.down,
        Vector2.left
    };

    protected override void StateMachine()
    {
        switch (enemyStates)
        {
            case EnemyStates.Idle:
                Idle();
                break;
        }
    }

    protected override void Idle()
    {
        rb2d.velocity = Vector2.zero;
        anim.SetFloat("speed", 0f);

        rotateTimer += Time.deltaTime;
        if (rotateTimer >= rotateInterval)
        {
            rotateTimer = 0f;
            directionIndex = (directionIndex + 1) % idleDirections.Length;

            Vector2 dir = idleDirections[directionIndex];

            anim.SetFloat("moveX", dir.x);
            anim.SetFloat("moveY", dir.y);

            lastMoveDir = dir;
        }
    }
}
