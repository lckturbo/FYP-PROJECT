using UnityEngine;

/// <summary>
/// Persistent container for the latest buff amount.
/// </summary>
public class BuffData : MonoBehaviour
{
    public static BuffData instance;

    public int latestAttackBuff = 0;  // Amount of buff to remove
    public bool hasBuff = false;       // Whether a buff exists

    private void Awake()
    {
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

    public void StoreBuff(int amount)
    {
        latestAttackBuff = amount;
        hasBuff = true;
    }

    public void ClearBuff()
    {
        latestAttackBuff = 0;
        hasBuff = false;
    }
}
