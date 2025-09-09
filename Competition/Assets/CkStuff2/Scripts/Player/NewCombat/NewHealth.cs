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
            ApplyStats(stats);
    }

    public void ApplyStats(NewCharacterStats newStats)
    {
        stats = newStats;
        if (useStatsDirectly && stats != null)
            currentHp = stats.maxHealth; // reset to full when applied
    }

    public void TakeDamage(int rawDamage, NewCharacterStats attacker, NewElementType skillElement = NewElementType.None)
    {
        if (stats == null) return;

        // 0) Choose which element the attack uses (skill overrides attacker element if specified)
        NewElementType atkElem = (skillElement != NewElementType.None) ? skillElement : (attacker != null ? attacker.element : NewElementType.None);

        // 1) Start with base damage (either provided rawDamage, or attacker.baseDamage if 0)
        float dmg = (rawDamage > 0) ? rawDamage : (attacker != null ? attacker.baseDamage : 1);

        // 2) Crit
        if (attacker != null && Random.value < Mathf.Clamp01(attacker.critRate))
        {
            dmg *= attacker.critDamage; // 1.5 => +50% total
            //Debug.Log("CRIT!");
        }

        // 3) Defense (flat). If you prefer %, change this to dmg *= (100f / (100f + stats.defense))
        dmg -= stats.defense;
        if (dmg < 1f) dmg = 1f;

        // 4) Element advantage triangle (type vs type)
        dmg *= GetElementTriangleMultiplier(atkElem, stats.element);

        // 5) Defender's elemental resistance table (fine tuning per character)
        dmg *= stats.GetResistance(atkElem);

        int final = Mathf.Max(1, Mathf.RoundToInt(dmg));

        // 6) Apply
        currentHp -= final;
        currentHp = Mathf.Max(0, currentHp);

        Debug.Log("(Player) Current HP: " + currentHp + "/" + stats.maxHealth);

        if (currentHp <= 0)
            Die();
    }

    private float GetElementTriangleMultiplier(NewElementType attack, NewElementType defense)
    {
        // Advantage (1.5x)
        if (attack == NewElementType.Fire && defense == NewElementType.Grass) return 1.5f;
        if (attack == NewElementType.Grass && defense == NewElementType.Water) return 1.5f;
        if (attack == NewElementType.Water && defense == NewElementType.Fire) return 1.5f;

        // Disadvantage (0.75x)
        if (attack == NewElementType.Grass && defense == NewElementType.Fire) return 0.75f;
        if (attack == NewElementType.Water && defense == NewElementType.Grass) return 0.75f;
        if (attack == NewElementType.Fire && defense == NewElementType.Water) return 0.75f;

        // Neutral
        return 1f;
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has died.");
        // TODO: death anim, disable controls, signal game state, etc.
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
