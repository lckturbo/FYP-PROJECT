using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Stats", fileName = "EnemyStats")]
public class EnemyStats : ScriptableObject
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
    [Header("Health")]
    public int maxHealth;
    [Header("Combat")]
    public int atkDmg;
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
