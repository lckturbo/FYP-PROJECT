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
    }

    private void OnDisable()
    {
        BattleManager.OnClearAllBuffs -= RemoveStoredBuff;
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

        if (duration > 0)
        {
            buffEndTime = Time.time + duration;  // set expiry timestamp
        }
        else
        {
            buffEndTime = 0f; // permanent until cleared manually
        }
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
