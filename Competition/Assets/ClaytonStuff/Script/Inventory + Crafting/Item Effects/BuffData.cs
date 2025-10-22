using UnityEngine;

public class BuffData : MonoBehaviour
{
    public static BuffData instance;

    public int latestAttackBuff;
    public NewCharacterStats attackTarget;
    public bool hasAttackBuff;

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

    public void StoreAttackBuff(int amount, NewCharacterStats target)
    {
        latestAttackBuff = amount;
        attackTarget = target;
        hasAttackBuff = true;
    }

    public void StoreDefenseBuff(int amount, NewCharacterStats target)
    {
        latestDefenseBuff = amount;
        defenseTarget = target;
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
