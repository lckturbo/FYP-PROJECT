using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("EnemyStats")]
    [SerializeField] private EnemyStats[] _enemies;

    [Header("SpawnStats")]
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private int totalEnemiesToSpawn;

    private List<GameObject> activeEnemies = new List<GameObject>();

    private void Start()
    {
        for (int i = 0; i < totalEnemiesToSpawn; i++)
            SpawnEnemy();
    }
    void SpawnEnemy()
    {
        int spawnPointsID = Random.Range(0, _spawnPoints.Length);
        EnemyStats randomEnemy = _enemies[Random.Range(0, _enemies.Length)];

        GameObject enemy = Instantiate(randomEnemy.enemyPrefab, _spawnPoints[spawnPointsID].position, Quaternion.identity);

        EnemyBase _enemyBase = enemy.GetComponent<EnemyBase>();
        //_enemyBase.OnDeath += RemoveEnemyFromList;

        activeEnemies.Add(enemy);
    }

    void RemoveEnemyFromList(GameObject enemy, float delay)
    {
        Debug.Log(delay);
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            StartCoroutine(RespawnAfterDelay(delay));
        }
    }

    private IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (activeEnemies.Count < totalEnemiesToSpawn)
            SpawnEnemy();
    }

    //private void OnDisable()
    //{
    //    foreach(GameObject e in activeEnemies)
    //    {
    //        if (e == null) continue;
    //        EnemyBase _enemyBase = e.GetComponent<EnemyBase>();
    //        if(_enemyBase != null) 
    //            _enemyBase.OnDeath -= RemoveEnemyFromList;
    //    }
    //}
}
