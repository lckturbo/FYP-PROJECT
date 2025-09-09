using System.Runtime.CompilerServices;
using UnityEngine;

public class RangeEnemy : EnemyBase
{
    [SerializeField] private Transform _shootPt;
    protected override void Attack()
    {
        base.Attack();
        Debug.Log("(Enemy) Attack State");
        PlayerNearby();

        // TODO: ANIMATIONS

        if (_currAtkTimer <= _atkCD)
        {
            _currAtkTimer -= Time.deltaTime;
            if (_currAtkTimer <= 0f)
            {
                //Projectile(); // TODO: TEMP -> SET IN ANIMATION EVENTS
                _currAtkTimer = _atkCD;
            }
        }
    }

    protected override void BattleAttack()
    {
        throw new System.NotImplementedException();
    }

    private void Projectile()
    {
        Vector2 dir = player.position - _shootPt.position;

        GameObject proj = ProjectilePool.instance.GetProjectile();
        proj.transform.position = _shootPt.position;
        // projectile face player
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0, 0, angle); 
        proj.SetActive(true);
        proj.GetComponent<EnemyProjectile>().Init(dir, _atkDmg);
    }
}
