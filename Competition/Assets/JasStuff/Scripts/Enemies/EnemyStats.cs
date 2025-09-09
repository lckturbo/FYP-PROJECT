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
    [Header("Combat")]
    public float atkCD;
    public int atkRange;
    public float dmgReduction;

    [Header("Boss Stats")]
    public float AOERadius;

    [Header("FSM stats")]
    public float idleTimer;
    public float chaseRange;
    public float investigateTimer;
}
