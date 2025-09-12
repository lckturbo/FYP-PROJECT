using System.Collections;
using UnityEngine;

public class AOEBoss : EnemyBase
{
    //[SerializeField] private Transform _warnPt;
    //[SerializeField] private GameObject _aoeWarningPrefab;
    //[SerializeField] private float _windupTime;
    ////[SerializeField] private GameObject _aoeEffectPrefab; // show area hit

    //protected override void Attack()
    //{
    //    base.Attack();
    //    if (_currAtkTimer <= _atkCD)
    //    {
    //        _currAtkTimer -= Time.deltaTime;
    //        if (_currAtkTimer <= 0)
    //        {
    //            StartCoroutine(PerformAOEWarn());
    //            _currAtkTimer = _atkCD;

    //        }
    //    }
    //}

    //protected override void BattleAttack()
    //{
    //    throw new System.NotImplementedException();
    //}

    //private IEnumerator PerformAOEWarn()
    //{
    //    MsgLog("AOEBoss Attack");

    //    // WARN PLAYER ABOUT AOE -> ENEMY WINDING UP ATTACK // 
    //    if (_aoeWarningPrefab != null)
    //    {
    //        MsgLog("AOE warning");
    //        GameObject warn = Instantiate(_aoeWarningPrefab, _warnPt);
    //        // TODO: AOE WARNING EFFECTS
    //        Vector3 scale = warn.transform.localScale;
    //        scale.x *= _AOERadius * 2f;
    //        warn.transform.localScale = scale;
    //        Destroy(warn, _windupTime);
    //    }

    //    yield return new WaitForSeconds(_windupTime);

    //    // TODO: AOE ATTACK EFFECT
    //    // TODO: ANIMATIONS

    //    // ATTACK PLAYER //
    //    Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, _AOERadius, LayerMask.GetMask("Player"));
    //    foreach (Collider2D h in hit)
    //    {
    //        MsgLog("AOE hit: " + h.gameObject.name);
    //        //if (h.TryGetComponent<Health>(out Health playerHealth))
    //        //    playerHealth.TakeDamage(_atkDmg);
    //    }
    //}
    protected override void BattleAttack()
    {
        throw new System.NotImplementedException();
    }
}
