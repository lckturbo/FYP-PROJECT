using UnityEngine;
using System.Collections;

public class PlayerBuffHandler : MonoBehaviour
{
    [SerializeField] private NewCharacterStats stats;

    private int currentAttackBuff = 0;
    private Coroutine activeBuffRoutine;
    private bool buffActive = false;

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

        currentAttackBuff += amount;
        stats.atkDmg += amount;
        buffActive = true;

        // Store amount to BuffData for later removal
        BuffData.instance?.StoreBuff(amount);

        if (duration > 0)
            activeBuffRoutine = StartCoroutine(AttackBuffRoutine(duration));
    }

    private IEnumerator AttackBuffRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        buffActive = false;
        activeBuffRoutine = null;
        // BuffData still keeps the amount until battle ends
    }

    private void RemoveStoredBuff()
    {
        if (BuffData.instance != null && BuffData.instance.hasBuff)
        {
            int amount = BuffData.instance.latestAttackBuff;

            stats.atkDmg -= amount;
            currentAttackBuff -= amount;
            buffActive = false;

            BuffData.instance.ClearBuff();

            Debug.Log($"Buff removed: -{amount} atk");
        }
    }
}
