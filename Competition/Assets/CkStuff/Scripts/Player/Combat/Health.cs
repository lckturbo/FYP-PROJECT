using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Character Stats")]
    public CharacterStats stats;
    public bool useStatsDirectly = true;

    [Header("Runtime Health")]
    public int currentHp;

    void Awake()
    {
        if (stats != null)
        {
            ApplyStats(stats);
        }
    }

    public void ApplyStats(CharacterStats newStats)
    {
        stats = newStats;

        if (useStatsDirectly)
        {
            if (currentHp <= 0) currentHp = stats.maxHealth;
            currentHp = Mathf.Min(currentHp, stats.maxHealth);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Max(0, currentHp);

        if (currentHp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " has died.");
        // TODO: trigger death animation, end turn, etc.
    }
}