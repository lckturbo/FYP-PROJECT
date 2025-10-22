using System.Collections;
using UnityEngine;

public class NewHealth : MonoBehaviour
{
    [Header("Character Stats")]
    [SerializeField] private BaseStats stats;
    [SerializeField] private bool useStatsDirectly = true;
    [SerializeField] private Animator anim;

    [Header("Damage Flash")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color defaultFlashColor = Color.red; // fallback color
    [SerializeField] private float flashDuration = 0.25f;
    private Color originalColor;
    private Coroutine flashRoutine;

    private int currentHp;

    public event System.Action<NewHealth> OnHealthChanged;
    public event System.Action<NewHealth> OnDeathComplete;

    [Header("Damage Numbers")]
    [SerializeField] private GameObject floatingDamagePrefab;
    [SerializeField] private Transform damageSpawnPoint;
    private bool canSpawnDamage = true;

    public int GetCurrHealth() => currentHp;
    public int GetMaxHealth() => stats ? stats.maxHealth : 1;

    void Awake()
    {
        if (stats != null) ApplyStats(stats);

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

    private IEnumerator SpawnFloatingDamageWithDelay(int final, bool wasCrit)
    {
        canSpawnDamage = false;
        Vector3 spawnPos = damageSpawnPoint ? damageSpawnPoint.position : transform.position + Vector3.up * 1.5f;

        var dmgObj = Instantiate(floatingDamagePrefab, spawnPos, Quaternion.identity);
        dmgObj.GetComponent<FloatingDamage>()?.Initialize(final, wasCrit);

        yield return new WaitForSeconds(0.15f);
        canSpawnDamage = true;
    }

    public void TakeDamage(int rawDamage, BaseStats attacker, NewElementType skillElement = NewElementType.None)
    {
        if (stats == null) return;

        // --- 0) Inputs / defaults ---
        float baseAtk = (rawDamage > 0) ? rawDamage : (attacker != null ? attacker.atkDmg : 1);
        NewElementType atkElem = (skillElement != NewElementType.None)
            ? skillElement
            : (attacker != null ? attacker.attackElement : NewElementType.None);

        // --- 1) Flat defense ---
        float def = Mathf.Max(0f, stats.attackreduction);
        float dmg = baseAtk - def;
        if (dmg < 0f) dmg = 0f;

        // --- 2) Crit ---
        bool wasCrit = false;
        if (attacker != null && Random.value < Mathf.Clamp01(attacker.critRate))
        {
            wasCrit = true;
            float critMult = 1f + Mathf.Max(0f, attacker.critDamage);
            dmg *= critMult;
        }

        // --- 3) Elemental multiplier ---
        float tri = GetElementTriangleMultiplier(atkElem, stats.defenseElement);
        float res = stats.GetResistance(atkElem);
        dmg *= (tri * res);

        // --- 4) Final ---
        int final = Mathf.Max(1, Mathf.RoundToInt(dmg));
        currentHp = Mathf.Max(0, currentHp - final);

        Debug.Log($"{gameObject.name} took {final} {atkElem} damage. HP = {currentHp}/{GetMaxHealth()}");

        if (floatingDamagePrefab && canSpawnDamage)
            StartCoroutine(SpawnFloatingDamageWithDelay(final, wasCrit));

        // --- 5) Trigger damage flash depending on attacker ---
        if (spriteRenderer)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            Color flash = GetFlashColor(attacker);
            flashRoutine = StartCoroutine(FlashDamage(flash));
        }

        OnHealthChanged?.Invoke(this);
        if (currentHp <= 0) Die();
    }

    private IEnumerator FlashDamage(Color color)
    {
        spriteRenderer.color = color;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    private Color GetFlashColor(BaseStats attacker)
    {
        if (attacker == null) return defaultFlashColor;

        //  Example logic — you can customize this freely:
        if (attacker.attackElement == NewElementType.Fire) return new Color(1f, 0.3f, 0.3f); // red
        if (attacker.attackElement == NewElementType.Water) return new Color(0.3f, 0.6f, 1f); // blue
        if (attacker.attackElement == NewElementType.Grass) return new Color(0.3f, 1f, 0.3f); // green
        if (attacker.attackElement == NewElementType.Dark) return new Color(0.5f, 0f, 0.8f);  // purple
        if (attacker.attackElement == NewElementType.Light) return new Color(1f, 1f, 0.6f);   // yellowish
        return defaultFlashColor;
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

    public void Heal(int amount)
    {
        if (stats == null) return;

        currentHp = Mathf.Min(currentHp + amount, stats.maxHealth);
        Debug.Log($"{gameObject.name} healed {amount} HP. Current HP = {currentHp}/{stats.maxHealth}");
        OnHealthChanged?.Invoke(this);
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
