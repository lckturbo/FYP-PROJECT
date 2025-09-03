using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Stats", fileName = "EnemyStats")]
public class EnemyStats : ScriptableObject
{
    [Header("Speed")]
    public int speed;
    [Header("Health")]
    public int maxHealth;
    [Header("Combat")]
    public int atkDmg;
    public int atkCD;
    public int atkRange;

    [Header("FSM stats")]
    public float idleTimer;
    public float chaseRange;
    public float investigateTimer;

}
