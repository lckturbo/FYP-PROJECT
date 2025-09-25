using UnityEngine;

public class NewHealth : MonoBehaviour
{
    [Header("Character Stats")]
    [SerializeField] private BaseStats stats;
    [SerializeField] private bool useStatsDirectly = true;

    private int currentHp;

    public event System.Action<NewHealth> OnHealthChanged;

    public int GetCurrHealth() => currentHp;
    public int GetMaxHealth() => stats ? stats.maxHealth : 1;

    void Awake()
    {
        if (stats != null) ApplyStats(stats);
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

        // 1) Which element?
        NewElementType atkElem = (skillElement != NewElementType.None)
            ? skillElement
            : (attacker != null ? attacker.attackElement : NewElementType.None);

        // 2) Base damage
        float dmg = (rawDamage > 0) ? rawDamage : (attacker != null ? attacker.atkDmg : 1);

        // 3) Crit (use UnityEngine.Random explicitly)
        if (attacker != null && UnityEngine.Random.value < Mathf.Clamp01(attacker.critRate))
        {
            dmg *= attacker.critDamage;
            Debug.Log($"{attacker.name} landed a CRIT!");
        }

        // 4) Defense
        dmg -= stats.attackreduction;
        if (dmg < 1f) dmg = 1f;

        // 5) Element triangle
        dmg *= GetElementTriangleMultiplier(atkElem, stats.defenseElement);

        // 6) Per-character resistance
        dmg *= stats.GetResistance(atkElem);

        // 7) Apply
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
        OnHealthChanged?.Invoke(this);
        Destroy(gameObject, 1f);
    }
}
