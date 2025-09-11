using UnityEngine;

public class NewHealth : MonoBehaviour
{
    [Header("Character Stats")]
    [SerializeField] private BaseStats stats;
    [SerializeField] private bool useStatsDirectly = true;

    private int currentHp;

    void Awake()
    {
        if (stats != null)
            ApplyStats(stats);
    }

    public void ApplyStats(BaseStats newStats)
    {
        stats = newStats;
        if (useStatsDirectly && stats != null)
            currentHp = stats.maxHealth; // reset to full when applied
    }

    public void TakeDamage(int rawDamage, BaseStats attacker, NewElementType skillElement = NewElementType.None)
    {
        if (stats == null) return;

        // --- Step 1: Which element is this attack?
        NewElementType atkElem = (skillElement != NewElementType.None)
            ? skillElement
            : (attacker != null ? attacker.attackElement : NewElementType.None);

        // --- Step 2: Base damage
        float dmg = (rawDamage > 0) ? rawDamage : (attacker != null ? attacker.atkDmg : 1);

        // --- Step 3: Crit
        if (attacker != null && Random.value < Mathf.Clamp01(attacker.critRate))
        {
            dmg *= attacker.critDamage;
            Debug.Log(attacker.characterName + " landed a CRIT!");
        }

        // --- Step 4: Defense
        dmg -= stats.defense;
        if (dmg < 1f) dmg = 1f;

        // --- Step 5: Elemental triangle (attack vs defense element)
        dmg *= GetElementTriangleMultiplier(atkElem, stats.defenseElement);

        // --- Step 6: Per-character resistance tuning
        dmg *= stats.GetResistance(atkElem);

        // --- Step 7: Apply
        int final = Mathf.Max(1, Mathf.RoundToInt(dmg));
        currentHp -= final;
        currentHp = Mathf.Max(0, currentHp);

        Debug.Log($"{gameObject.name} took {final} {atkElem} damage. HP = {currentHp}/{stats.maxHealth}");

        if (currentHp <= 0)
            Die();
    }

    private float GetElementTriangleMultiplier(NewElementType attack, NewElementType defense)
    {
        // Example advantage system
        if (attack == NewElementType.Fire && defense == NewElementType.Grass) return 1.5f;
        if (attack == NewElementType.Grass && defense == NewElementType.Water) return 1.5f;
        if (attack == NewElementType.Water && defense == NewElementType.Fire) return 1.5f;

        if (attack == NewElementType.Grass && defense == NewElementType.Fire) return 0.75f;
        if (attack == NewElementType.Water && defense == NewElementType.Grass) return 0.75f;
        if (attack == NewElementType.Fire && defense == NewElementType.Water) return 0.75f;

        // Dark vs Light (special rule)
        if (attack == NewElementType.Dark && defense == NewElementType.Light) return 1.5f;
        if (attack == NewElementType.Light && defense == NewElementType.Dark) return 1.5f;

        return 1f; // neutral
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has died.");
        // TODO: death anim, disable movement, signal game state, etc.
    }

    // temp -> jas added
    public int GetMaxHealth()
    {
        return stats.maxHealth;
    }
    public int GetCurrHealth()
    {
        return currentHp;
    }
}
