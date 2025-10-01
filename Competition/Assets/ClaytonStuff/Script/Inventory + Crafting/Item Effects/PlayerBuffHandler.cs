using UnityEngine;
using System.Collections;

public class PlayerBuffHandler : MonoBehaviour
{
    [SerializeField] private NewCharacterStats stats;

    private int currentAttackBuff = 0;
    private Coroutine activeBuffRoutine;
    private bool buffActive = false;

    public bool IsBuffActive => buffActive;

    // On your player prefab root
    private void Awake()
    {
        if (stats != null) ApplyStats(stats);
        DontDestroyOnLoad(gameObject);
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

        // Apply all base stats to the character
        stats.atkDmg = newStats.atkDmg;
        stats.maxHealth = newStats.maxHealth;

        Debug.Log("Stats applied to player.");
    }


    public void ApplyAttackBuff(int amount, float duration)
    {
        if (stats == null) return;

        if (buffActive)
        {
            Debug.Log("Buff already active! Wait until it expires.");
            return;
        }

        activeBuffRoutine = StartCoroutine(AttackBuffRoutine(amount, duration));
    }

    private IEnumerator AttackBuffRoutine(int amount, float duration)
    {
        buffActive = true;

        currentAttackBuff += amount;
        stats.atkDmg += amount;
        Debug.Log($"Attack buff applied: +{amount} atk for {duration}s");

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
        Debug.Log("All buffs cleared.");
    }


}
