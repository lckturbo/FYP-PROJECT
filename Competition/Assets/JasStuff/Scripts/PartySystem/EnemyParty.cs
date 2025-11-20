using System.Collections.Generic;
using UnityEngine;
using static GameData;

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

    //public void LoadData(GameData data)
    //{
    //    foreach (var entry in data.enemyPositions)
    //    {
    //        if (entry.enemyID == uniqueID)
    //        {
    //            transform.position = new Vector2(entry.x, entry.y);
    //            return;
    //        }
    //    }
    //}

    //public void SaveData(ref GameData data)
    //{
    //    // Update if exists
    //    foreach (var entry in data.enemyPositions)
    //    {
    //        if (entry.enemyID == uniqueID)
    //        {
    //            entry.x = transform.position.x;
    //            entry.y = transform.position.y;
    //            return;
    //        }
    //    }

    //    // Otherwise add new
    //    data.enemyPositions.Add(new EnemyPositionData
    //    {
    //        enemyID = uniqueID,
    //        x = transform.position.x,
    //        y = transform.position.y
    //    });
    //}
}
