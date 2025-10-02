using UnityEngine;
using System.Collections;

public class PlayerBuffHandler : MonoBehaviour
{
    [SerializeField] private NewCharacterStats stats;

    private int currentAttackBuff = 0;
    private Coroutine activeBuffRoutine;
    private bool buffActive = false;

    public bool IsBuffActive => buffActive;

    private void Awake()
    {
        if (stats != null) ApplyStats(stats);

        // If a buff was active before scene reload, reapply it
        if (BuffData.instance != null && BuffData.instance.isBuffActive)
        {
            ApplyAttackBuff(BuffData.instance.latestAttackBuff, 0); // duration 0 means instant apply
        }
    }

    private void OnEnable()
    {
        BattleManager.OnClearAllBuffs += ClearAllBuffs;
    }

    private void OnDisable()
    {
        BattleManager.OnClearAllBuffs -= ClearAllBuffs;
    }

    public void ApplyStats(NewCharacterStats newStats)
    {
        if (newStats == null) return;

        stats = newStats;
        stats.atkDmg = newStats.atkDmg;
        stats.maxHealth = newStats.maxHealth;

        Debug.Log("Stats applied to player.");
    }

    public void ApplyAttackBuff(int amount, float duration)
    {
        if (stats == null) return;

        if (buffActive)
            return;

        currentAttackBuff += amount;
        stats.atkDmg += amount;
        buffActive = true;

        // Store in BuffData
        if (BuffData.instance != null)
            BuffData.instance.SetBuff(currentAttackBuff);

        Debug.Log($"Attack buff applied: +{amount} atk for {duration}s");

        if (duration > 0)
            activeBuffRoutine = StartCoroutine(AttackBuffRoutine(amount, duration));
    }

    private IEnumerator AttackBuffRoutine(int amount, float duration)
    {
        yield return new WaitForSeconds(duration);
        RemoveBuff(amount);
    }

    private void RemoveBuff(int amount)
    {
        if (!buffActive) return;

        stats.atkDmg -= amount;
        currentAttackBuff -= amount;
        buffActive = false;
        activeBuffRoutine = null;

        // Clear BuffData since it's removed
        if (BuffData.instance != null)
            BuffData.instance.ClearBuff();

        Debug.Log("Attack buff expired.");
    }

    public void ClearAllBuffs()
    {
        if (activeBuffRoutine != null)
        {
            StopCoroutine(activeBuffRoutine);
            activeBuffRoutine = null;
        }

        if (currentAttackBuff > 0)
        {
            stats.atkDmg -= currentAttackBuff;
            currentAttackBuff = 0;
        }

        buffActive = false;

        // Clear BuffData
        if (BuffData.instance != null)
            BuffData.instance.ClearBuff();

        Debug.Log("All buffs cleared.");
    }
}
