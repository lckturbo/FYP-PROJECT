using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Stats/EnemyStats")]
public class EnemyStats : BaseStats
{
    public enum EnemyTypes
    {
        Basic,
        MiniBoss
    }

    [Header("EnemyType")]
    public EnemyTypes type;
    [Header("EnemyPrefab")]
    public GameObject enemyPrefab;
    public GameObject bossPrefab;
    public GameObject basicEnemyPrefab;
    [Header("EnemyCombat")]
    public float atkRange;
    [Header("BossStats")]
    public float AOERadius;
    [Header("FSMStats")]
    public float chaseRange;
}
