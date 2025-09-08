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
            currentHp = stats.maxHealth;
        }
    }

    public void TakeDamage(int rawDamage, ElementType attackElement = ElementType.None)
    {
        if (stats == null) return;

        int effectiveDamage = Mathf.Max(1, rawDamage - stats.defense);

        float elementMultiplier = GetElementMultiplier(attackElement, stats.element);
        effectiveDamage = Mathf.RoundToInt(effectiveDamage * elementMultiplier);

        currentHp -= effectiveDamage;
        currentHp = Mathf.Max(0, currentHp);

        Debug.Log("(Player) Current HP: " + currentHp + "/" + stats.maxHealth);

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private float GetElementMultiplier(ElementType attack, ElementType defense)
    {
        // Example
        if (attack == ElementType.Fire && defense == ElementType.Grass) return 1.5f;
        if (attack == ElementType.Grass && defense == ElementType.Water) return 1.5f;
        if (attack == ElementType.Water && defense == ElementType.Fire) return 1.5f;

        // Weakness
        if (attack == ElementType.Grass && defense == ElementType.Fire) return 0.75f;
        if (attack == ElementType.Water && defense == ElementType.Grass) return 0.75f;
        if (attack == ElementType.Fire && defense == ElementType.Water) return 0.75f;

        return 1f; // neutral
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has died.");
        // TODO: trigger death animation, disable movement, end turn, etc.
    }
}
