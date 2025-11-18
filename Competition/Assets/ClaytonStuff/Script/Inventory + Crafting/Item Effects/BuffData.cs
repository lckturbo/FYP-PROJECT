using System.Collections.Generic;
using UnityEngine;

public class BuffData : MonoBehaviour
{
    public static BuffData instance;

    // OLD COMPAT
    public int latestAttackBuff;
    public NewCharacterStats attackTarget;
    public bool hasAttackBuff;

    public int latestDefenseBuff;
    public NewCharacterStats defenseTarget;
    public bool hasDefenseBuff;

    // NEW MULTI-BUFF SUPPORT
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


    // ---------- ADD BUFF ----------
    public void AddBuff(BuffEntry.BuffType type, int amount, NewCharacterStats target)
    {
        allBuffs.Add(new BuffEntry()
        {
            type = type,
            amount = amount,
            target = target
        });
    }

    // ---------- REMOVE EXACT BUFF ----------
    public void RemoveBuff(BuffEntry entry)
    {
        allBuffs.Remove(entry);
    }

    // ---------- REMOVE ALL BUFFS FOR A SPECIFIC TARGET ----------
    public void RemoveBuffsForTarget(NewCharacterStats target)
    {
        allBuffs.RemoveAll(b => b.target == target);
    }

    // ---------- REMOVE ONE BUFF OF A CERTAIN TYPE FOR A TARGET ----------
    public void RemoveBuff(NewCharacterStats target, BuffEntry.BuffType type)
    {
        for (int i = 0; i < allBuffs.Count; i++)
        {
            if (allBuffs[i].target == target && allBuffs[i].type == type)
            {
                allBuffs.RemoveAt(i);
                return; // remove ONLY ONE
            }
        }
    }

    // ---------- OLD API ----------
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
        RemoveBuff(attackTarget, BuffEntry.BuffType.Attack);

        latestAttackBuff = 0;
        attackTarget = null;
        hasAttackBuff = false;
    }

    public void ClearDefenseBuff()
    {
        RemoveBuff(defenseTarget, BuffEntry.BuffType.Defense);

        latestDefenseBuff = 0;
        defenseTarget = null;
        hasDefenseBuff = false;
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
