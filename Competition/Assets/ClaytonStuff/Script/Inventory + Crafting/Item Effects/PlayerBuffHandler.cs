using UnityEngine;

public class PlayerBuffHandler : MonoBehaviour
{
    [SerializeField] private NewCharacterStats stats;

    private int currentAttackBuff = 0;
    private bool buffActive = false;
    private float buffEndTime = 0f;   // timestamp when buff ends

    public bool IsBuffActive => buffActive;

    private void OnEnable()
    {
        BattleManager.OnClearAllBuffs += RemoveStoredBuff;

        // Automatically assign the correct stats based on the current leader
        AssignLeaderStats();
    }

    private void OnDisable()
    {
        BattleManager.OnClearAllBuffs -= RemoveStoredBuff;
    }

    /// <summary>
    /// Automatically finds and assigns stats based on the current party leader.
    /// </summary>
    private void AssignLeaderStats()
    {
        if (PlayerParty.instance == null)
        {
            Debug.LogWarning($"{name}: No PlayerParty found, cannot assign leader stats.");
            return;
        }

        var leaderDef = PlayerParty.instance.GetLeader();
        if (leaderDef == null)
        {
            Debug.LogWarning($"{name}: No leader assigned in PlayerParty!");
            return;
        }

        // Get the leader’s runtime stats (usually inside the leader prefab)
        stats = leaderDef.runtimeStats;
        if (stats != null)
        {
            Debug.Log($"[{name}] Assigned leader stats: {stats.name} (ATK {stats.atkDmg}, HP {stats.maxHealth})");
        }
        else
        {
            Debug.LogWarning($"{name}: Leader definition has no runtimeStats assigned!");
        }
    }

    public void ApplyStats(NewCharacterStats newStats)
    {
        if (newStats == null) return;

        stats = newStats;
        stats.atkDmg = newStats.atkDmg;
        stats.maxHealth = newStats.maxHealth;
    }

    public void ApplyAttackBuff(int amount, float duration)
    {
        if (stats == null || buffActive) return;

        currentAttackBuff = amount;
        stats.atkDmg += amount;
        buffActive = true;

        // Store amount + stats reference to BuffData for later cleanup
        BuffData.instance?.StoreBuff(amount, stats);

        buffEndTime = (duration > 0) ? Time.time + duration : 0f;
    }

    private void Update()
    {
        if (buffActive && buffEndTime > 0f && Time.time >= buffEndTime)
        {
            // Expire buff when time is up
            stats.atkDmg -= currentAttackBuff;
            Debug.Log($"Buff expired: -{currentAttackBuff} atk");

            currentAttackBuff = 0;
            buffActive = false;

            BuffData.instance?.ClearBuff();
        }
    }

    public void RemoveStoredBuff()
    {
        if (BuffData.instance != null && BuffData.instance.hasBuff)
        {
            int amount = BuffData.instance.latestAttackBuff;
            NewCharacterStats target = BuffData.instance.targetStats;

            if (target != null)
            {
                target.atkDmg -= amount;
                Debug.LogWarning($"[Buff Removed] Character Stats: {target.name}, -{amount} atk, atk now {target.atkDmg}");
            }

            currentAttackBuff = 0;
            buffActive = false;
            BuffData.instance.ClearBuff();
        }
    }
}
