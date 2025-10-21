using UnityEngine;

/// <summary>
/// Persistent container for the latest buff amount and target stats.
/// </summary>
public class BuffData : MonoBehaviour
{
    public static BuffData instance;

    public int latestAttackBuff = 0;        // Amount of buff to remove
    public NewCharacterStats targetStats;   // Which character stats this buff belongs to
    public bool hasBuff = false;            // Whether a buff exists

    private void Awake()
    {
        // If this GameObject is a child
        if (transform.parent != null)
        {
            // Detach from its parent first
            transform.SetParent(null);
        }
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

    public void StoreBuff(int amount, NewCharacterStats stats)
    {
        latestAttackBuff = amount;
        targetStats = stats;
        hasBuff = true;
    }

    public void ClearBuff()
    {
        latestAttackBuff = 0;
        targetStats = null;
        hasBuff = false;
    }
}
