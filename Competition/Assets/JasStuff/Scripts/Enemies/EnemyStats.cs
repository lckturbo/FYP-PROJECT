using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Stats/EnemyStats")]
public class EnemyStats : BaseStats
{
    public enum EnemyTypes
    {
        Basic,
        Tank,
        MiniBoss
    }

    [Header("EnemyType")]
    public EnemyTypes type;
    [Header("EnemyPrefab")]
    public GameObject enemyPrefab;
    [Header("EnemyCombat")]
    public float atkRange;
    [Header("BossStats")]
    public float AOERadius;
    [Header("FSMStats")]
    public float idleTimer;
    public float chaseRange;
}
