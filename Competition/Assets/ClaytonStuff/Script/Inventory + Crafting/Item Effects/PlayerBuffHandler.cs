using UnityEngine;
using System.Collections;

public class PlayerBuffHandler : MonoBehaviour
{
    [SerializeField] private NewCharacterStats stats; // drag and drop in Inspector
    private int currentAttackBuff = 0;

    public void ApplyAttackBuff(int amount, float duration)
    {
        if (stats == null) return;
        StopAllCoroutines();
        StartCoroutine(AttackBuffRoutine(amount, duration));
    }

    private IEnumerator AttackBuffRoutine(int amount, float duration)
    {
        currentAttackBuff += amount;
        stats.atkDmg += amount;
        Debug.Log($"Attack buff applied: +{amount} atk for {duration}s");

        yield return new WaitForSeconds(duration);

        stats.atkDmg -= amount;
        currentAttackBuff -= amount;
        Debug.Log("Attack buff expired.");
    }
}

