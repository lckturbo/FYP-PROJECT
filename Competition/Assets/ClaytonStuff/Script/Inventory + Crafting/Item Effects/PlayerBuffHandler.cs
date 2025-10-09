using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Handles applying attack buffs to the current leader's runtime stats.
/// Buffs are automatically tracked via BuffData.
/// </summary>
public class PlayerBuffHandler : MonoBehaviour
{
    [SerializeField] private NewCharacterStats stats;

    private int currentAttackBuff = 0;
    private bool buffActive = false;
    private float buffEndTime = 0f;

    public bool IsBuffActive => buffActive;

    private void OnEnable()
    {
        BattleManager.OnClearAllBuffs += RemoveStoredBuff;

        // Auto-assign leader stats
        AssignLeaderStats();
    }

    private void OnDisable()
    {
        BattleManager.OnClearAllBuffs -= RemoveStoredBuff;
    }

    /// <summary>
    /// Finds the current party leader and assigns their runtime stats.
    /// </summary>
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

        // Pull runtime stats from leader's PlayerLevelApplier
        var levelApplier = leaderDef.GetComponent<PlayerLevelApplier>();
        if (levelApplier != null && levelApplier.runtimeStats != null)
        {
            stats = levelApplier.runtimeStats;
            Debug.Log($"[{name}] Assigned leader stats: {stats.name} (ATK {stats.atkDmg}, HP {stats.maxHealth})");
        }
        else
        {
            Debug.LogWarning($"{name}: Leader has no runtimeStats available!");
        }
    }

    /// <summary>
    /// Apply a direct attack buff to the leader's runtime stats.
    /// </summary>
    public void ApplyAttackBuff(int amount, float duration)
    {
        if (stats == null)
        {
            Debug.LogWarning($"{name}: Cannot apply buff, stats not assigned!");
            return;
        }

        if (buffActive)
        {
            Debug.LogWarning($"{name}: Buff already active, cannot stack!");
            return;
        }

        currentAttackBuff = amount;
        stats.atkDmg += amount;
        buffActive = true;

        // Store reference in BuffData for persistence
        BuffData.instance?.StoreBuff(amount, stats);

        buffEndTime = (duration > 0f) ? Time.time + duration : 0f;
        Debug.Log($"Buff applied: +{amount} ATK to {stats.name} for {(duration > 0 ? duration + "s" : "permanent")}");
    }

    private void Update()
    {
        if (buffActive && buffEndTime > 0f && Time.time >= buffEndTime)
        {
            ExpireBuff();
        }
    }

    /// <summary>
    /// Manually remove the stored buff.
    /// </summary>
    public void RemoveStoredBuff()
    {
        if (BuffData.instance != null && BuffData.instance.hasBuff)
        {
            int amount = BuffData.instance.latestAttackBuff;
            NewCharacterStats target = BuffData.instance.targetStats;

            if (target != null)
            {
                target.atkDmg -= amount;
                Debug.LogWarning($"[Buff Removed] {target.name} lost {amount} ATK, new ATK: {target.atkDmg}");
            }

            currentAttackBuff = 0;
            buffActive = false;
            BuffData.instance.ClearBuff();
        }
    }

    /// <summary>
    /// Internal helper to expire timed buffs.
    /// </summary>
    private void ExpireBuff()
    {
        stats.atkDmg -= currentAttackBuff;
        Debug.Log($"Buff expired: -{currentAttackBuff} ATK from {stats.name}");

        currentAttackBuff = 0;
        buffActive = false;
        BuffData.instance?.ClearBuff();
    }
}
