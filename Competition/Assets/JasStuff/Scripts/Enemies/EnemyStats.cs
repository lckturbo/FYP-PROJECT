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
    [Header("Speed")]
    public int speed;
    [Header("EnemyCombat")]
    public float atkCD;
    public float atkRange;
    [Header("BossStats")]
    public float dmgReduction;
    public float AOERadius;
    [Header("FSMStats")]
    public float idleTimer;
    public float chaseRange;
    public float investigateTimer;
}
