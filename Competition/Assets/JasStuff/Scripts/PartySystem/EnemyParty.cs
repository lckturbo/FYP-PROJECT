using System.Collections.Generic;
using UnityEngine;

public class EnemyParty : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private string uniqueID;
    private int partySize = 3;

    public string GetID() => uniqueID;

    private EnemyStats enemyStats;

    private void Start()
    {
        EnemyBase enemyBase = GetComponent<EnemyBase>();
        if (!enemyBase) return;

        enemyStats = enemyBase.GetEnemyStats();

        // fallback if prefab not set manually
        if (!_enemyPrefab && enemyStats != null)
        {
            _enemyPrefab = enemyStats.basicEnemyPrefab
                           ? enemyStats.basicEnemyPrefab
                           : enemyStats.bossPrefab;
        }
    }

    public List<GameObject> GetEnemies()
    {
        List<GameObject> enemies = new List<GameObject>();

        if (enemyStats == null)
        {
            Debug.LogWarning($"No EnemyStats found on {name}");
            return enemies;
        }

        // ---- BOSS PARTY ---- //
        if (enemyStats.type == EnemyStats.EnemyTypes.MiniBoss)
        {
            Debug.Log($"[{name}] is a Boss-type enemy. Creating boss + minions.");

            if (!enemyStats.bossPrefab)
                Debug.LogWarning($"[{name}] Missing bossPrefab in {enemyStats.name}");
            if (!_enemyPrefab)
                Debug.LogWarning($"[{name}] Missing _enemyPrefab reference!");
            if (!enemyStats.basicEnemyPrefab)
                Debug.LogWarning($"[{name}] Missing basicEnemyPrefab in {enemyStats.name}");

            GameObject bossPrefab = enemyStats.bossPrefab ? enemyStats.bossPrefab : _enemyPrefab;
            if (bossPrefab)
                enemies.Add(bossPrefab);
            else
                Debug.LogError($"[{name}] Boss prefab is null!");

            for (int i = 0; i < 2; i++)
            {
                GameObject minion = enemyStats.basicEnemyPrefab
                    ? enemyStats.basicEnemyPrefab
                    : _enemyPrefab;

                if (minion)
                    enemies.Add(minion);
                else
                    Debug.LogError($"[{name}] Minion prefab is null!");
            }
        }
        else
        {
            for (int i = 0; i < partySize; i++)
                enemies.Add(_enemyPrefab);
        }

        return enemies;
    }
}
