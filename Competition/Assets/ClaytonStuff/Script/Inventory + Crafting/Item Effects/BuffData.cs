using UnityEngine;

public class BuffData : MonoBehaviour
{
    public static BuffData instance;

    // Attack buff
    public int latestAttackBuff;
    public NewCharacterStats attackTarget;
    public bool hasAttackBuff;

    // Defense buff
    public int latestDefenseBuff;
    public NewCharacterStats defenseTarget;
    public bool hasDefenseBuff;

    private void Awake()
    {
        if (transform.parent != null)
            transform.SetParent(null);

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StoreAttackBuff(int amount, NewCharacterStats stats)
    {
        latestAttackBuff = amount;
        attackTarget = stats;
        hasAttackBuff = true;
    }

    public void StoreDefenseBuff(int amount, NewCharacterStats stats)
    {
        latestDefenseBuff = amount;
        defenseTarget = stats;
        hasDefenseBuff = true;
    }

    public void ClearAttackBuff()
    {
        latestAttackBuff = 0;
        attackTarget = null;
        hasAttackBuff = false;
    }

    public void ClearDefenseBuff()
    {
        latestDefenseBuff = 0;
        defenseTarget = null;
        hasDefenseBuff = false;
    }
}
