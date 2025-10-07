using System.Collections;
using UnityEngine;

public class NewHealth : MonoBehaviour
{
    [Header("Character Stats")]
    [SerializeField] private BaseStats stats;
    [SerializeField] private bool useStatsDirectly = true;
    [SerializeField] private Animator anim;

    private int currentHp;

    public event System.Action<NewHealth> OnHealthChanged;
    public event System.Action<NewHealth> OnDeathComplete;

    public int GetCurrHealth() => currentHp;
    public int GetMaxHealth() => stats ? stats.maxHealth : 1;

    void Awake()
    {
        if (stats != null) ApplyStats(stats);
    }

    private void Start()
    {
        if (!anim) anim = GetComponent<Animator>();
    }

    public void ApplyStats(BaseStats newStats)
    {
        stats = newStats;
        if (useStatsDirectly && stats != null)
            currentHp = stats.maxHealth;

        OnHealthChanged?.Invoke(this);
    }

    public void TakeDamage(int rawDamage, BaseStats attacker, NewElementType skillElement = NewElementType.None)
    {
        if (stats == null) return;

        // --- 0) Inputs / defaults ---
        float baseAtk = (rawDamage > 0) ? rawDamage : (attacker != null ? attacker.atkDmg : 1);
        NewElementType atkElem = (skillElement != NewElementType.None)
            ? skillElement
            : (attacker != null ? attacker.attackElement : NewElementType.None);

        // --- 1) Flat defense BEFORE multipliers ---
        float def = Mathf.Max(0f, stats.attackreduction);
        float dmg = baseAtk - def;                 // shave off some raw damage first
        if (dmg < 0f) dmg = 0f;                    // don't go negative before mults

        // --- 2) Crit (BONUS: 0.5 => +50%) ---
        if (attacker != null && UnityEngine.Random.value < Mathf.Clamp01(attacker.critRate))
        {
            float critBonus = Mathf.Max(0f, attacker.critDamage);
            float critMult = 1f + critBonus;
            dmg *= critMult;
            Debug.Log($"{attacker?.name ?? "Unknown"} CRIT x{critMult:0.##}!");
        }

        // --- 3) Element triangle + per-character resistance ---
        float tri = GetElementTriangleMultiplier(atkElem, stats.defenseElement);
        float res = stats.GetResistance(atkElem);
        dmg *= (tri * res);

        // --- 4) Final clamp & apply ---
        int final = Mathf.Max(1, Mathf.RoundToInt(dmg));
        currentHp = Mathf.Max(0, currentHp - final);

        Debug.Log($"{gameObject.name} took {final} {atkElem} damage. HP = {currentHp}/{GetMaxHealth()}");
        OnHealthChanged?.Invoke(this);
        if (currentHp <= 0) Die();
    }

    private float GetElementTriangleMultiplier(NewElementType attack, NewElementType defense)
    {
        if (attack == NewElementType.Fire && defense == NewElementType.Grass) return 1.5f;
        if (attack == NewElementType.Grass && defense == NewElementType.Water) return 1.5f;
        if (attack == NewElementType.Water && defense == NewElementType.Fire) return 1.5f;

        if (attack == NewElementType.Grass && defense == NewElementType.Fire) return 0.75f;
        if (attack == NewElementType.Water && defense == NewElementType.Grass) return 0.75f;
        if (attack == NewElementType.Fire && defense == NewElementType.Water) return 0.75f;

        if (attack == NewElementType.Dark && defense == NewElementType.Light) return 1.5f;
        if (attack == NewElementType.Light && defense == NewElementType.Dark) return 1.5f;

        return 1f;
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has died.");
        if (anim) anim.SetTrigger("death");

        OnHealthChanged?.Invoke(this);
        Destroy(gameObject, 0.1f);
        //Debug.LogWarning("enemy die");
    }

    public void EndDeath()
    {
        OnDeathComplete?.Invoke(this);
        Destroy(gameObject, 0.1f);
        //Debug.Log("Enemy die");
    }
}
