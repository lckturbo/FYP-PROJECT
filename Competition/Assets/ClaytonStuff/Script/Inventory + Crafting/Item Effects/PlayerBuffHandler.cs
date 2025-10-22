using Unity.VisualScripting;
using UnityEngine;

public class PlayerBuffHandler : MonoBehaviour
{
    [SerializeField] private NewCharacterStats stats;
    [SerializeField] private GameObject buffEffect; // for attack buff
    [SerializeField] private GameObject defenseBuffEffect; // optional separate VFX

    private int currentAttackBuff = 0;
    private int currentDefenseBuff = 0;
    private bool attackBuffActive = false;
    private bool defenseBuffActive = false;
    private float attackBuffEndTime = 0f;
    private float defenseBuffEndTime = 0f;

    public bool IsBuffActive => attackBuffActive || defenseBuffActive;

    private void Start()
    {
        UpdateBuffEffects();
    }

    private void OnEnable()
    {
        BattleManager.OnClearAllBuffs += RemoveStoredBuffs;
        AssignLeaderStats();
    }

    private void OnDisable()
    {
        BattleManager.OnClearAllBuffs -= RemoveStoredBuffs;
    }

    public void AssignLeaderStats()
    {
        if (PlayerParty.instance == null)
        {
            Debug.LogWarning($"{name}: No PlayerParty found.");
            return;
        }

        var leaderDef = PlayerParty.instance.GetLeader();
        if (leaderDef == null)
        {
            Debug.LogWarning($"{name}: No leader assigned in PlayerParty!");
            return;
        }

        var levelApplier = leaderDef.GetComponent<PlayerLevelApplier>();
        if (levelApplier != null && levelApplier.runtimeStats != null)
        {
            stats = levelApplier.runtimeStats;
        }
    }

    public void ApplyAttackBuff(int amount, float duration)
    {
        if (stats == null || attackBuffActive) return;

        currentAttackBuff = amount;
        stats.atkDmg += amount;
        attackBuffActive = true;
        attackBuffEndTime = duration > 0 ? Time.time + duration : 0f;

        BuffData.instance?.StoreAttackBuff(amount, stats);
        Debug.Log($"Applied +{amount} ATK for {duration}s.");
        UpdateBuffEffects();
    }

    public void ApplyDefenseBuff(int amount, float duration)
    {
        if (stats == null || defenseBuffActive) return;

        currentDefenseBuff = amount;
        stats.attackreduction += amount;
        defenseBuffActive = true;
        defenseBuffEndTime = duration > 0 ? Time.time + duration : 0f;

        BuffData.instance?.StoreDefenseBuff(amount, stats);
        Debug.Log($"Applied +{amount} DEF for {duration}s.");
        UpdateBuffEffects();
    }

    private void Update()
    {
        if (attackBuffActive && attackBuffEndTime > 0 && Time.time >= attackBuffEndTime)
            ExpireAttackBuff();
        if (defenseBuffActive && defenseBuffEndTime > 0 && Time.time >= defenseBuffEndTime)
            ExpireDefenseBuff();
    }

    private void ExpireAttackBuff()
    {
        stats.atkDmg -= currentAttackBuff;
        currentAttackBuff = 0;
        attackBuffActive = false;
        BuffData.instance?.ClearAttackBuff();
        Debug.Log("Attack buff expired.");
        UpdateBuffEffects();
    }

    private void ExpireDefenseBuff()
    {
        stats.attackreduction -= currentDefenseBuff;
        currentDefenseBuff = 0;
        defenseBuffActive = false;
        BuffData.instance?.ClearDefenseBuff();
        Debug.Log("Defense buff expired.");
        UpdateBuffEffects();
    }

    public void RemoveStoredBuffs()
    {
        if (BuffData.instance != null)
        {
            if (BuffData.instance.hasAttackBuff)
            {
                stats.atkDmg -= BuffData.instance.latestAttackBuff;
                BuffData.instance.ClearAttackBuff();
            }

            if (BuffData.instance.hasDefenseBuff)
            {
                stats.attackreduction -= BuffData.instance.latestDefenseBuff;
                BuffData.instance.ClearDefenseBuff();
            }

            currentAttackBuff = 0;
            currentDefenseBuff = 0;
            attackBuffActive = false;
            defenseBuffActive = false;
            UpdateBuffEffects();
        }
    }

    private void UpdateBuffEffects()
    {
        if (buffEffect != null)
            buffEffect.SetActive(BuffData.instance.hasAttackBuff);

        if (defenseBuffEffect != null)
            defenseBuffEffect.SetActive(BuffData.instance.hasDefenseBuff);
    }
}
