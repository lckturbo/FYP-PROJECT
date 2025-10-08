using System.Collections.Generic;
using UnityEngine;

public class EnemyParty : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab;
    private int partySize = 3;

    [SerializeField] private string uniqueID;
    public string GetID() => uniqueID;

    private void Start()
    {
        if (!_enemyPrefab)
            _enemyPrefab = GetComponent<EnemyBase>().GetEnemyStats().enemyPrefab;
    }
    public List<GameObject> GetEnemies()
    {
        List<GameObject> enemies = new List<GameObject>();
        for (int i = 0; i < partySize; i++)
            enemies.Add(_enemyPrefab);
        return enemies;
    }
}
