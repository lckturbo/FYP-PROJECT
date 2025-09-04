using System.Runtime.CompilerServices;
using UnityEngine;

public class RangeEnemy : EnemyBase
{
    [SerializeField] private Transform shootPt;
    [SerializeField] private GameObject projectilePrefab;
    protected override void Attack()
    {
        base.Attack();
        Debug.Log("(Enemy) Attack State");
        PlayerNearby();

        // play animation

        if (_currAtkTimer <= _atkCD)
        {
            _currAtkTimer -= Time.deltaTime;
            if (_currAtkTimer <= 0f)
            {
                Projectile();
                _currAtkTimer = _atkCD;
            }
        }
    }

    private void Projectile()
    {
        Vector2 dir = player.position - shootPt.position;

        // !! object pooling
        GameObject proj = ProjectilePool.instance.GetProjectile();
        proj.transform.position = shootPt.position;
        // projectile face player
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0, 0, angle);
        // !! set projectile active here 
        proj.SetActive(true);
        proj.GetComponent<EnemyProjectile>().Init(dir, _atkDmg);
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

    //void OnDrawGizmosSelected()
    //{
    //    if (shootPt.gameObject)
    //    {
    //        Gizmos.color = Color.blue;
    //        Gizmos.DrawWireSphere(shootPt.position, 0.2f);
    //    }
    //}
}
