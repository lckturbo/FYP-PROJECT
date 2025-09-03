using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Character Stats")]
    [SerializeField] private CharacterStats stats;
    [SerializeField] private bool useStatsDirectly = true;

    private int currentHp;

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

        Debug.Log("(Player) Current HP: " + currentHp + "/" + stats.maxHealth);

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