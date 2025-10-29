using System.Collections.Generic;
using UnityEngine;

public class EnemyParty : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private string uniqueID;
    [SerializeField] private int partySize = 3;

    public string GetID() => uniqueID;

    private EnemyStats enemyStats;

    private void Start()
    {
        EnemyBase enemyBase = GetComponent<EnemyBase>();
        if (!enemyBase) return;

        enemyStats = enemyBase.GetEnemyStats();
        if (!_enemyPrefab && enemyStats != null)
            _enemyPrefab = enemyStats.enemyPrefab;
    }

    public List<GameObject> GetEnemies()
    {
        List<GameObject> enemies = new List<GameObject>();

        if (enemyStats == null)
            return enemies;


        for (int i = 0; i < partySize; i++)
            enemies.Add(_enemyPrefab);

        return enemies;
    }
}
