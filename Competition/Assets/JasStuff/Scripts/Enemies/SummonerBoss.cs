using System.Collections.Generic;
using UnityEngine;

public class SummonerBoss : EnemyBase
{
    [SerializeField] private GameObject[] _enemiesPrefab;
    [SerializeField] private Transform[] _summonPts;
    private int _maxEnemies = 5;

    private List<GameObject> _activeEnemies = new List<GameObject>();
    // summon minions
    // summon -> attack -> summon
    // if more than 5 basic summoned, will stop summoning, resume summoning when 1 basic dies
    protected override void Attack()
    {
        base.Attack();
        if (_activeEnemies.Count <= _maxEnemies)
        {
            int eCount = Random.Range(0, _enemiesPrefab.Length);
            int sCount = Random.Range(0, _summonPts.Length);
            GameObject e = Instantiate(_enemiesPrefab[eCount], _summonPts[sCount].position, Quaternion.identity);
            _activeEnemies.Add(e);

            //if (e.TryGetComponent<EnemyBase>(out EnemyBase minions))
            //{
            //    minions.OnDeath += () =>
            //    {
            //        _activeEnemies.Remove(e);
            //        MsgLog("Minions died, active enemies: " + _activeEnemies.Count);
            //    };
            //}

        }
    }

    protected override void BattleAttack()
    {
        throw new System.NotImplementedException();
    }
}
