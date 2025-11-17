using System.Collections.Generic;
using UnityEngine;

public class BuffData : MonoBehaviour
{
    public static BuffData instance;

    // OLD (kept for compatibility)
    public int latestAttackBuff;
    public NewCharacterStats attackTarget;
    public bool hasAttackBuff;

    public int latestDefenseBuff;
    public NewCharacterStats defenseTarget;
    public bool hasDefenseBuff;

    // NEW — MULTI-BUFF SUPPORT
    [System.Serializable]
    public class BuffEntry
    {
        public enum BuffType { Attack, Defense }
        public BuffType type;
        public int amount;
        public NewCharacterStats target;
    }

    public List<BuffEntry> allBuffs = new List<BuffEntry>();


    private void Awake()
    {
        if (transform.parent != null)
            transform.SetParent(null);

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // OLD API (kept to avoid breaking anything)
    public void StoreAttackBuff(int amount, NewCharacterStats target)
    {
        latestAttackBuff = amount;
        attackTarget = target;
        hasAttackBuff = true;

        AddBuff(BuffEntry.BuffType.Attack, amount, target);
    }

    public void StoreDefenseBuff(int amount, NewCharacterStats target)
    {
        latestDefenseBuff = amount;
        defenseTarget = target;
        hasDefenseBuff = true;

        AddBuff(BuffEntry.BuffType.Defense, amount, target);
    }

    public void ClearAttackBuff()
    {
        latestAttackBuff = 0;
        attackTarget = null;
        hasAttackBuff = false;

        RemoveBuffType(BuffEntry.BuffType.Attack);
    }

    public void ClearDefenseBuff()
    {
        latestDefenseBuff = 0;
        defenseTarget = null;
        hasDefenseBuff = false;

        RemoveBuffType(BuffEntry.BuffType.Defense);
    }

    // NEW API
    public void AddBuff(BuffEntry.BuffType type, int amount, NewCharacterStats target)
    {
        allBuffs.Add(new BuffEntry()
        {
            type = type,
            amount = amount,
            target = target
        });
    }

    public void RemoveBuffType(BuffEntry.BuffType type)
    {
        allBuffs.RemoveAll(b => b.type == type);
    }

    public void ClearAllBuffs()
    {
        allBuffs.Clear();
        latestAttackBuff = 0;
        latestDefenseBuff = 0;
        attackTarget = null;
        defenseTarget = null;
        hasAttackBuff = false;
        hasDefenseBuff = false;
    }
}
