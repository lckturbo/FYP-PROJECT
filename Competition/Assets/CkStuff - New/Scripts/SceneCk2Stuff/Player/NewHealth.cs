using System.Collections;
using UnityEngine;

public class NewHealth : MonoBehaviour
{
    [Header("Character Stats")]
    [SerializeField] private BaseStats stats;
    [SerializeField] private bool useStatsDirectly = true;
    [SerializeField] private Animator anim;

    //  Add reference for sprite renderer
    [Header("Damage Flash")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 1.0f;
    private Color originalColor;
    private Coroutine flashRoutine;

    private int currentHp;

    public event System.Action<NewHealth> OnHealthChanged;
    public event System.Action<NewHealth> OnDeathComplete;

    public int GetCurrHealth() => currentHp;
    public int GetMaxHealth() => stats ? stats.maxHealth : 1;

    void Awake()
    {
        if (stats != null) ApplyStats(stats);

        //  Try to auto-grab a SpriteRenderer
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer) originalColor = spriteRenderer.color;
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
        float dmg = baseAtk - def;
        if (dmg < 0f) dmg = 0f;

        // --- 2) Crit ---
        if (attacker != null && UnityEngine.Random.value < Mathf.Clamp01(attacker.critRate))
        {
            float critBonus = Mathf.Max(0f, attacker.critDamage);
            float critMult = 1f + critBonus;
            dmg *= critMult;
            Debug.Log($"{attacker?.name ?? "Unknown"} CRIT x{critMult:0.##}!");
        }

        // --- 3) Element triangle + resistance ---
        float tri = GetElementTriangleMultiplier(atkElem, stats.defenseElement);
        float res = stats.GetResistance(atkElem);
        dmg *= (tri * res);

        // --- 4) Final clamp & apply ---
        int final = Mathf.Max(1, Mathf.RoundToInt(dmg));
        currentHp = Mathf.Max(0, currentHp - final);

        Debug.Log($"{gameObject.name} took {final} {atkElem} damage. HP = {currentHp}/{GetMaxHealth()}");

        //  Trigger damage flash
        if (spriteRenderer)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashDamage());
        }

        OnHealthChanged?.Invoke(this);
        if (currentHp <= 0) Die();
    }

    private IEnumerator FlashDamage()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
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
    }

    public void EndDeath()
    {
        OnDeathComplete?.Invoke(this);
        Destroy(gameObject, 0.1f);
    }
}
