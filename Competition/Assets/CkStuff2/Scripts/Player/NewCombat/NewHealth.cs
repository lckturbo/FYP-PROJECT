using UnityEngine;

public class NewHealth : MonoBehaviour
{
    [Header("Character Stats")]
    [SerializeField] private NewCharacterStats stats;
    [SerializeField] private bool useStatsDirectly = true;

    private int currentHp;

    void Awake()
    {
        if (stats != null)
        {
            ApplyStats(stats);
        }
    }

    public void ApplyStats(NewCharacterStats newStats)
    {
        stats = newStats;

        if (useStatsDirectly && stats != null)
        {
            currentHp = stats.maxHealth;
        }
    }

    public void TakeDamage(int rawDamage, NewElementType attackElement = NewElementType.None)
    {
        if (stats == null) return;

        int effectiveDamage = Mathf.Max(1, rawDamage - stats.defense);

        float elementMultiplier = GetElementMultiplier(attackElement, stats.element);
        effectiveDamage = Mathf.RoundToInt(effectiveDamage * elementMultiplier);

        currentHp -= effectiveDamage;
        currentHp = Mathf.Max(0, currentHp);

        Debug.Log($"(Player) Current HP: {currentHp}/{stats.maxHealth}");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private float GetElementMultiplier(NewElementType attack, NewElementType defense)
    {
        // Example triangle
        if (attack == NewElementType.Fire && defense == NewElementType.Grass) return 1.5f;
        if (attack == NewElementType.Grass && defense == NewElementType.Water) return 1.5f;
        if (attack == NewElementType.Water && defense == NewElementType.Fire) return 1.5f;

        if (attack == NewElementType.Grass && defense == NewElementType.Fire) return 0.75f;
        if (attack == NewElementType.Water && defense == NewElementType.Grass) return 0.75f;
        if (attack == NewElementType.Fire && defense == NewElementType.Water) return 0.75f;

        return 1f; // neutral
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has died.");
        // TODO: trigger death animation, disable movement, etc.
    }
}
